# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

import sys
import time
import uuid
from datetime import timedelta
from typing import Callable, cast
from unittest.mock import Mock, patch

import pytest
from azure.monitor.query import LogsQueryClient, LogsQueryResult

from geh_settlement_report.entry_points.entry_point import (
    _start_task,
)
from geh_settlement_report.entry_points.job_args.calculation_type import CalculationType
from geh_settlement_report.entry_points.tasks.task_type import TaskType
from tests.integration_test_configuration import IntegrationTestConfiguration


class TestWhenInvokedWithArguments:
    def test_add_info_log_record_to_azure_monitor_with_expected_settings(
        self,
        integration_test_configuration: IntegrationTestConfiguration,
        script_args_fixture_integration_test,
        clean_up_logging,
    ) -> None:
        """
        Assert that the settlement report job adds log records to Azure Monitor with the expected settings:
        | where AppRoleName == "dbr-settlement-report"
        | where SeverityLevel == 1
        | where Message startswith_cs "Command line arguments"
        | where OperationId != "00000000000000000000000000000000"
        | where Properties.Subsystem == "settlement-report-aggregations"
        - custom field "settlement_report_id" = <the settlement report id>
        - custom field "CategoryName" = "Energinet.DataHub." + <logger name>

        Debug level is not tested as it is not intended to be logged by default.
        """

        # Arrange
        valid_task_type = TaskType.TimeSeriesHourly
        script_args = script_args_fixture_integration_test
        new_report_id = str(uuid.uuid4())
        updated_args = update_script_arg(script_args, "--report-id", new_report_id)

        applicationinsights_connection_string = (
            integration_test_configuration.get_applicationinsights_connection_string()
        )

        task_factory_mock = Mock()

        # Act
        with patch(
            "geh_settlement_report.entry_points.tasks.task_factory.create",
            task_factory_mock,
        ):
            with (
                patch(
                    "geh_settlement_report.entry_points.tasks.time_series_points_task.TimeSeriesPointsTask.execute",
                    return_value=None,
                ),
                patch("sys.argv", updated_args),
                patch.dict(
                    "os.environ",
                    {"APPLICATIONINSIGHTS_CONNECTION_STRING": applicationinsights_connection_string},
                ),
            ):
                _start_task(task_type=valid_task_type)

        # Assert
        # noinspection PyTypeChecker
        logs_client = LogsQueryClient(integration_test_configuration.credential)

        query = f"""
        AppTraces
        | where AppRoleName == "dbr-settlement-report"
        | where SeverityLevel == 1
        | where Message startswith_cs "Command line arguments"
        | where OperationId != "00000000000000000000000000000000"
        | where Properties.Subsystem == "settlement-report-aggregations"
        | where Properties.settlement_report_id == "{new_report_id}"
        | where Properties.CategoryName == "Energinet.DataHub.geh_settlement_report.entry_points.entry_point"
        | count
        """

        workspace_id = integration_test_configuration.get_analytics_workspace_id()

        def assert_logged():
            actual = logs_client.query_workspace(workspace_id, query, timespan=timedelta(minutes=5))
            err_msg = f"The expected log record was not found in Azure Monitor using query:\n{query}"
            assert_row_count(
                actual,
                1,
                error_message=err_msg,
            )

        # Assert, but timeout if not succeeded
        wait_for_condition(assert_logged, timeout=timedelta(minutes=5), step=timedelta(seconds=10))

    def test_add_exception_log_record_to_azure_monitor_with_unexpected_settings(
        self,
        integration_test_configuration: IntegrationTestConfiguration,
        script_args_fixture_integration_test,
        clean_up_logging,
    ) -> None:
        """
        Assert that the settlement report job adds log records to Azure Monitor with the expected settings:
        | where AppRoleName == "dbr-settlement-report"
        | where ExceptionType == "argparse.ArgumentTypeError"
        | where OuterMessage startswith_cs "Grid area codes must consist of 3 digits"
        | where OperationId != "00000000000000000000000000000000"
        | where Properties.Subsystem == "settlement-report-aggregations"
        - custom field "settlement_report_id" = <the settlement report id>
        - custom field "CategoryName" = "Energinet.DataHub." + <logger name>

        Debug level is not tested as it is not intended to be logged by default.
        """

        # Arrange
        valid_task_type = TaskType.TimeSeriesHourly
        script_args = script_args_fixture_integration_test  # Get the original fixture values
        new_report_id = str(uuid.uuid4())
        updated_args = update_script_arg(script_args, "--report-id", new_report_id)
        updated_args = update_script_arg(updated_args, "--calculation-type", CalculationType.BALANCE_FIXING.value)
        if "--grid-area-codes" not in updated_args:
            updated_args.extend(["--grid-area-codes", "[8054]"])

        applicationinsights_connection_string = (
            integration_test_configuration.get_applicationinsights_connection_string()
        )
        task_factory_mock = Mock()

        # Act
        with pytest.raises(SystemExit):
            with patch(
                "geh_settlement_report.entry_points.tasks.task_factory.create",
                task_factory_mock,
            ):
                with (
                    patch(
                        "geh_settlement_report.entry_points.tasks.time_series_points_task.TimeSeriesPointsTask.execute",
                        return_value=None,
                    ),
                    patch("sys.argv", updated_args),
                    patch.dict(
                        "os.environ",
                        {
                            "APPLICATIONINSIGHTS_CONNECTION_STRING": applicationinsights_connection_string,
                            "CATALOG_NAME": "test_catalog",
                        },
                    ),
                ):
                    _start_task(task_type=valid_task_type)

        # Assert
        # noinspection PyTypeChecker
        logs_client = LogsQueryClient(integration_test_configuration.credential)

        query = f"""
        AppExceptions
        | where AppRoleName == "dbr-settlement-report"
        | where ExceptionType == "pydantic_core._pydantic_core.ValidationError"
        | where OuterMessage contains "Unexpected grid area code"
        | where OperationId != "00000000000000000000000000000000"
        | where Properties.Subsystem == "settlement-report-aggregations"
        | where Properties.settlement_report_id == "{new_report_id}"
        | where Properties.CategoryName == "Energinet.DataHub.geh_common.telemetry.span_recording"
        | count
        """

        workspace_id = integration_test_configuration.get_analytics_workspace_id()

        def assert_logged():
            actual = logs_client.query_workspace(workspace_id, query, timespan=timedelta(minutes=5))
            err_msg = f"The expected log record was not found in Azure Monitor using query:\n{query}"
            # There should be two counts, one from the arg_parser and one
            assert_row_count(actual, 1, error_message=err_msg)

        # Assert, but timeout if not succeeded
        wait_for_condition(assert_logged, timeout=timedelta(minutes=5), step=timedelta(seconds=10))


def update_script_arg(args: list[str], param_name: str, new_value: str) -> list[str]:
    """
    Updates the value of a given parameter in the script arguments list.

    :param args: List of script arguments.
    :param param_name: The parameter to update (e.g., "--report-id").
    :param new_value: The new value to replace the existing one.
    :return: A new list with the updated parameter value.
    """
    updated_args = args[:]  # Copy the list to avoid modifying the original
    try:
        index = updated_args.index(param_name) + 1  # Find the parameter and get its value index
        updated_args[index] = new_value  # Replace with the new value
    except ValueError:
        pass  # If the parameter isn't found, do nothing
    return updated_args


def wait_for_condition(callback: Callable, *, timeout: timedelta, step: timedelta):
    """
    Wait for a condition to be met, or timeout.
    The function keeps invoking the callback until it returns without raising an exception.
    """
    start_time = time.time()
    while True:
        elapsed_ms = int((time.time() - start_time) * 1000)
        # noinspection PyBroadException
        try:
            callback()
            print(f"Condition met in {elapsed_ms} ms")  # noqa
            return
        except Exception:
            if elapsed_ms > timeout.total_seconds() * 1000:
                print(  # noqa
                    f"Condition failed to be met before timeout. Timed out after {elapsed_ms} ms",
                    file=sys.stderr,
                )
                raise
            time.sleep(step.seconds)
            print(f"Condition not met after {elapsed_ms} ms. Retrying...")  # noqa


def assert_row_count(actual, expected_count, error_message=None):
    actual = cast(LogsQueryResult, actual)
    table = actual.tables[0]
    row = table.rows[0]
    value = row["Count"]
    count = cast(int, value)
    assert count == expected_count, error_message
