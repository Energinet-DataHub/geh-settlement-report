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
import logging
import os
import shutil
import sys
import uuid
from pathlib import Path
from typing import Callable, Generator

import geh_common.telemetry.logging_configuration
import pytest
import yaml
from delta import configure_spark_with_delta_pip
from geh_common.telemetry.logging_configuration import (
    configure_logging,
)
from geh_common.testing.spark.mocks import MockDBUtils
from pyspark.sql import SparkSession

from geh_settlement_report.settlement_reports.application.job_args.calculation_type import CalculationType
from geh_settlement_report.settlement_reports.application.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from geh_settlement_report.settlement_reports.domain.utils.market_role import MarketRole
from tests.constants import PROJECT_PATH
from tests.data_seeding import (
    standard_balance_fixing_scenario_data_generator,
    standard_wholesale_fixing_scenario_data_generator,
)
from tests.data_seeding.write_test_data import (
    write_amounts_per_charge_to_delta_table,
    write_charge_link_periods_to_delta_table,
    write_charge_price_information_periods_to_delta_table,
    write_charge_price_points_to_delta_table,
    write_energy_per_es_to_delta_table,
    write_energy_to_delta_table,
    write_latest_calculations_by_day_to_delta_table,
    write_metering_point_periods_to_delta_table,
    write_metering_point_time_series_to_delta_table,
    write_monthly_amounts_per_charge_to_delta_table,
    write_total_monthly_amounts_to_delta_table,
)
from tests.integration_test_configuration import IntegrationTestConfiguration


@pytest.fixture(scope="session")
def dbutils() -> MockDBUtils:
    """
    Returns a DBUtilsFixture instance that can be used to mock dbutils.
    """
    return MockDBUtils()


@pytest.fixture(scope="session", autouse=True)
def cleanup_before_tests(
    input_database_location: str,
):
    if os.path.exists(input_database_location):
        shutil.rmtree(input_database_location)

    yield

    # Add cleanup code to be run after the tests


@pytest.fixture(scope="function")
def standard_wholesale_fixing_scenario_args(
    settlement_reports_output_path: str,
) -> SettlementReportArgs:
    return SettlementReportArgs(
        report_id=str(uuid.uuid4()),
        period_start=standard_wholesale_fixing_scenario_data_generator.FROM_DATE,
        period_end=standard_wholesale_fixing_scenario_data_generator.TO_DATE,
        calculation_type=CalculationType.WHOLESALE_FIXING,
        calculation_id_by_grid_area={
            standard_wholesale_fixing_scenario_data_generator.GRID_AREAS[0]: uuid.UUID(
                standard_wholesale_fixing_scenario_data_generator.CALCULATION_ID
            ),
            standard_wholesale_fixing_scenario_data_generator.GRID_AREAS[1]: uuid.UUID(
                standard_wholesale_fixing_scenario_data_generator.CALCULATION_ID
            ),
        },
        grid_area_codes=None,
        split_report_by_grid_area=True,
        prevent_large_text_files=False,
        time_zone="Europe/Copenhagen",
        catalog_name="spark_catalog",
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.SYSTEM_OPERATOR,  # using system operator since it is more complex (requires filter based on charge owner)
        requesting_actor_id=standard_wholesale_fixing_scenario_data_generator.CHARGE_OWNER_ID_WITHOUT_TAX,
        settlement_reports_output_path=settlement_reports_output_path,
        include_basis_data=True,
    )


@pytest.fixture(scope="function")
def standard_wholesale_fixing_scenario_datahub_admin_args(
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
) -> SettlementReportArgs:
    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = MarketRole.DATAHUB_ADMINISTRATOR
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = None
    return standard_wholesale_fixing_scenario_args


@pytest.fixture(scope="function")
def standard_wholesale_fixing_scenario_energy_supplier_args(
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
) -> SettlementReportArgs:
    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = MarketRole.ENERGY_SUPPLIER
    energy_supplier_id = standard_wholesale_fixing_scenario_data_generator.ENERGY_SUPPLIER_IDS[0]
    standard_wholesale_fixing_scenario_args.requesting_actor_id = energy_supplier_id
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = [energy_supplier_id]
    return standard_wholesale_fixing_scenario_args


@pytest.fixture(scope="function")
def standard_wholesale_fixing_scenario_grid_access_provider_args(
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
) -> SettlementReportArgs:
    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = MarketRole.GRID_ACCESS_PROVIDER
    standard_wholesale_fixing_scenario_args.requesting_actor_id = (
        standard_wholesale_fixing_scenario_data_generator.CHARGE_OWNER_ID_WITH_TAX
    )
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = None
    return standard_wholesale_fixing_scenario_args


@pytest.fixture(scope="function")
def standard_wholesale_fixing_scenario_system_operator_args(
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
) -> SettlementReportArgs:
    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = MarketRole.SYSTEM_OPERATOR
    standard_wholesale_fixing_scenario_args.requesting_actor_id = (
        standard_wholesale_fixing_scenario_data_generator.CHARGE_OWNER_ID_WITHOUT_TAX
    )
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = None
    return standard_wholesale_fixing_scenario_args


@pytest.fixture(scope="function")
def standard_balance_fixing_scenario_args(
    settlement_reports_output_path: str,
) -> SettlementReportArgs:
    return SettlementReportArgs(
        report_id=str(uuid.uuid4()),
        period_start=standard_balance_fixing_scenario_data_generator.FROM_DATE,
        period_end=standard_balance_fixing_scenario_data_generator.TO_DATE,
        calculation_type=CalculationType.BALANCE_FIXING,
        calculation_id_by_grid_area=None,
        grid_area_codes=standard_balance_fixing_scenario_data_generator.GRID_AREAS,
        split_report_by_grid_area=True,
        prevent_large_text_files=False,
        time_zone="Europe/Copenhagen",
        catalog_name="spark_catalog",
        energy_supplier_ids=None,
        requesting_actor_market_role=MarketRole.SYSTEM_OPERATOR,
        requesting_actor_id="1212121212121",
        settlement_reports_output_path=settlement_reports_output_path,
        include_basis_data=True,
    )


@pytest.fixture(scope="function")
def standard_balance_fixing_scenario_grid_access_provider_args(
    standard_balance_fixing_scenario_args: SettlementReportArgs,
) -> SettlementReportArgs:
    standard_balance_fixing_scenario_args.requesting_actor_market_role = MarketRole.GRID_ACCESS_PROVIDER
    standard_balance_fixing_scenario_args.requesting_actor_id = (
        standard_wholesale_fixing_scenario_data_generator.CHARGE_OWNER_ID_WITH_TAX
    )
    standard_balance_fixing_scenario_args.energy_supplier_ids = None
    return standard_balance_fixing_scenario_args


@pytest.fixture(scope="session")
def standard_balance_fixing_scenario_data_written_to_delta(
    spark: SparkSession,
    input_database_location: str,
) -> None:
    time_series_points_df = standard_balance_fixing_scenario_data_generator.create_metering_point_time_series(spark)
    write_metering_point_time_series_to_delta_table(spark, time_series_points_df, input_database_location)

    metering_point_periods = standard_balance_fixing_scenario_data_generator.create_metering_point_periods(spark)
    write_metering_point_periods_to_delta_table(spark, metering_point_periods, input_database_location)

    energy_df = standard_balance_fixing_scenario_data_generator.create_energy(spark)
    write_energy_to_delta_table(spark, energy_df, input_database_location)

    energy_per_es_df = standard_balance_fixing_scenario_data_generator.create_energy_per_es(spark)
    write_energy_per_es_to_delta_table(spark, energy_per_es_df, input_database_location)

    latest_calculations_by_day = standard_balance_fixing_scenario_data_generator.create_latest_calculations(spark)
    write_latest_calculations_by_day_to_delta_table(spark, latest_calculations_by_day, input_database_location)


@pytest.fixture(scope="session")
def standard_wholesale_fixing_scenario_data_written_to_delta(
    spark: SparkSession,
    input_database_location: str,
) -> None:
    metering_point_periods = standard_wholesale_fixing_scenario_data_generator.create_metering_point_periods(spark)
    write_metering_point_periods_to_delta_table(spark, metering_point_periods, input_database_location)

    time_series_points = standard_wholesale_fixing_scenario_data_generator.create_metering_point_time_series(spark)
    write_metering_point_time_series_to_delta_table(spark, time_series_points, input_database_location)

    charge_link_periods = standard_wholesale_fixing_scenario_data_generator.create_charge_link_periods(spark)
    write_charge_link_periods_to_delta_table(spark, charge_link_periods, input_database_location)

    charge_price_points = standard_wholesale_fixing_scenario_data_generator.create_charge_price_points(spark)
    write_charge_price_points_to_delta_table(spark, charge_price_points, input_database_location)

    charge_price_information_periods = (
        standard_wholesale_fixing_scenario_data_generator.create_charge_price_information_periods(spark)
    )
    write_charge_price_information_periods_to_delta_table(
        spark, charge_price_information_periods, input_database_location
    )

    energy = standard_wholesale_fixing_scenario_data_generator.create_energy(spark)
    write_energy_to_delta_table(spark, energy, input_database_location)

    energy_per_es = standard_wholesale_fixing_scenario_data_generator.create_energy_per_es(spark)
    write_energy_per_es_to_delta_table(spark, energy_per_es, input_database_location)

    amounts_per_charge = standard_wholesale_fixing_scenario_data_generator.create_amounts_per_charge(spark)
    write_amounts_per_charge_to_delta_table(spark, amounts_per_charge, input_database_location)

    monthly_amounts_per_charge_df = standard_wholesale_fixing_scenario_data_generator.create_monthly_amounts_per_charge(
        spark
    )
    write_monthly_amounts_per_charge_to_delta_table(spark, monthly_amounts_per_charge_df, input_database_location)
    total_monthly_amounts_df = standard_wholesale_fixing_scenario_data_generator.create_total_monthly_amounts(spark)
    write_total_monthly_amounts_to_delta_table(spark, total_monthly_amounts_df, input_database_location)


@pytest.fixture(scope="session")
def file_path_finder() -> Callable[[str], str]:
    """
    Returns the path of the file.
    Please note that this only works if current folder haven't been changed prior using
    `os.chdir()`. The correctness also relies on the prerequisite that this function is
    actually located in a file located directly in the tests folder.
    """

    def finder(file: str) -> str:
        return os.path.dirname(os.path.normpath(file))

    return finder


@pytest.fixture(scope="session")
def source_path(file_path_finder: Callable[[str], str]) -> str:
    """
    Returns the <repo-root>/source folder path.
    Please note that this only works if current folder haven't been changed prior using
    `os.chdir()`. The correctness also relies on the prerequisite that this function is
    actually located in a file located directly in the tests folder.
    """
    return file_path_finder(f"{__file__}/../..")


@pytest.fixture(scope="session")
def contracts_path() -> Path:
    """
    Returns the source/contract folder path.
    Please note that this only works if current folder haven't been changed prior using
    `os.chdir()`. The correctness also relies on the prerequisite that this function is
    actually located in a file located directly in the tests folder.
    """
    return PROJECT_PATH / "contracts"


@pytest.fixture(scope="session")
def test_files_folder_path(tests_path: str) -> str:
    return f"{tests_path}/test_files"


@pytest.fixture(scope="session")
def settlement_reports_output_path(data_lake_path: str) -> str:
    return f"{data_lake_path}/settlement_reports_output"


@pytest.fixture(scope="session")
def input_database_location(data_lake_path: str) -> str:
    return f"{data_lake_path}/input_database"


@pytest.fixture(scope="session")
def data_lake_path(tests_path: str, worker_id: str) -> str:
    return f"{tests_path}/__data_lake__/{worker_id}"


@pytest.fixture(scope="session")
def tests_path() -> str:
    """
    Returns the tests folder path.
    Please note that this only works if current folder haven't been changed prior using
    `os.chdir()`. The correctness also relies on the prerequisite that this function is
    actually located in a file located directly in the tests folder.
    """
    return f"{PROJECT_PATH}/tests"


@pytest.fixture(scope="session")
def spark(
    tests_path: str,
) -> Generator[SparkSession, None, None]:
    warehouse_location = f"{tests_path}/__spark-warehouse__"

    session = configure_spark_with_delta_pip(
        SparkSession.builder.config("spark.sql.warehouse.dir", warehouse_location)
        .config("spark.sql.streaming.schemaInference", True)
        .config("spark.ui.showConsoleProgress", "false")
        .config("spark.ui.enabled", "false")
        .config("spark.ui.dagGraph.retainedRootRDDs", "1")
        .config("spark.ui.retainedJobs", "1")
        .config("spark.ui.retainedStages", "1")
        .config("spark.ui.retainedTasks", "1")
        .config("spark.sql.ui.retainedExecutions", "1")
        .config("spark.worker.ui.retainedExecutors", "1")
        .config("spark.worker.ui.retainedDrivers", "1")
        .config("spark.default.parallelism", 1)
        .config("spark.driver.memory", "2g")
        .config("spark.executor.memory", "2g")
        .config("spark.rdd.compress", False)
        .config("spark.shuffle.compress", False)
        .config("spark.shuffle.spill.compress", False)
        .config("spark.sql.shuffle.partitions", 1)
        .config("spark.sql.extensions", "io.delta.sql.DeltaSparkSessionExtension")
        .config(
            "spark.sql.catalog.spark_catalog",
            "org.apache.spark.sql.delta.catalog.DeltaCatalog",
        )
    ).getOrCreate()

    yield session
    session.stop()


@pytest.fixture(scope="session")
def script_args_fixture() -> list[str]:
    sys_argv = [
        "program_name",
        "--force_configuration",
        "false",
        "--orchestration-instance-id",
        "4a540892-2c0a-46a9-9257-c4e13051d76a",
    ]
    return sys_argv


@pytest.fixture(scope="function")
def script_args_fixture_integration_test() -> list[str]:
    sys_argv = [
        "program_name",
        "--force_configuration",
        "false",
        "--orchestration-instance-id",
        str(uuid.uuid4()),
        "--report-id",
        str(uuid.uuid4()),
        "--period-start",
        "2024-01-01T23:00:00Z",
        "--period-end",
        "2024-01-02T23:00:00Z",
        "--calculation-type",
        "wholesale_fixing",
        "--requesting-actor-market-role",
        "system_operator",
        "--requesting-actor-id",
        "5790001330552",
        "--calculation-id-by-grid-area",
        f"804={uuid.uuid4()}",
        "--split-report-by-grid-area",
        "true",
        "--prevent-large-text-files",
        "false",
        "--time-zone",
        "Europe/Copenhagen",
        "--catalog-name",
        "spark_catalog",
        "--settlement-reports-output-path",
        "/workspaces/geh-settlement-report/source/settlement_report_python/tests/__data_lake__/master/settlement_reports_output",
        "--include-basis-data",
        "true",
    ]
    return sys_argv


@pytest.fixture(autouse=True)
def configure_dummy_logging(monkeypatch: pytest.MonkeyPatch) -> None:
    """Ensure that logging hooks don't fail due to _TRACER_NAME not being set."""

    env = {
        "CLOUD_ROLE_NAME": "test_role",
        "APPLICATIONINSIGHTS_CONNECTION_STRING": "connection_string",
        "SUBSYSTEM": "test_subsystem",
    }
    argv = [
        "program_name",
        "--force_configuration",
        "false",
        "--orchestration-instance-id",
        "4a540892-2c0a-46a9-9257-c4e13051d76a",
    ]

    monkeypatch.setattr(os, "environ", env)
    monkeypatch.setattr(sys, "argv", argv)
    monkeypatch.setattr(geh_common.telemetry.logging_configuration, "configure_logging", lambda *args, **kwargs: None)
    configure_logging(cloud_role_name=env["CLOUD_ROLE_NAME"], subsystem=env["SUBSYSTEM"])


@pytest.fixture(scope="function")
def clean_up_logging():
    """
    Function that cleans up the Logging module prior to running integration tests, so logging can be reconfigured after use of configure_dummy_logging fixture
    """
    from geh_common.telemetry.logging_configuration import (
        set_extras,
        set_is_instrumented,
        set_tracer,
        set_tracer_name,
    )

    set_extras({})
    set_is_instrumented(False)
    set_tracer(None)
    set_tracer_name("")


@pytest.fixture(scope="session")
def integration_test_configuration(tests_path: str) -> IntegrationTestConfiguration:
    """
    Load settings for integration tests either from a local YAML settings file or from environment variables.
    Proceeds even if certain Azure-related keys are not present in the settings file.
    """

    settings_file_path = Path(tests_path) / "integrationtest.local.settings.yml"

    def load_settings_from_env() -> dict:
        return {
            key: os.getenv(key)
            for key in [
                "AZURE_KEYVAULT_URL",
                "AZURE_CLIENT_ID",
                "AZURE_CLIENT_SECRET",
                "AZURE_TENANT_ID",
                "AZURE_SUBSCRIPTION_ID",
            ]
            if os.getenv(key) is not None
        }

    settings = _load_settings_from_file(settings_file_path) or load_settings_from_env()

    # Set environment variables from loaded settings
    for key, value in settings.items():
        if value is not None:
            os.environ[key] = value

    if "AZURE_KEYVAULT_URL" in settings:
        return IntegrationTestConfiguration(azure_keyvault_url=settings["AZURE_KEYVAULT_URL"])

    logging.error(
        f"Integration test configuration could not be loaded from {settings_file_path} or environment variables."
    )
    raise Exception(
        "Failed to load integration test settings. Ensure that the Azure Key Vault URL is provided in the settings file or as an environment variable."
    )


def _load_settings_from_file(file_path: Path) -> dict:
    if file_path.exists():
        with file_path.open() as stream:
            return yaml.safe_load(stream)
    else:
        return {}
