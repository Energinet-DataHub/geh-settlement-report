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

from geh_common.telemetry.decorators import start_trace
from geh_common.telemetry.logger import Logger
from geh_common.telemetry.logging_configuration import (
    add_extras,
    configure_logging,
)

from geh_settlement_report.settlement_reports.application.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from geh_settlement_report.settlement_reports.application.tasks import task_factory
from geh_settlement_report.settlement_reports.application.tasks.measurements_report_task import (
    start_measurements_report_with_deps,
)
from geh_settlement_report.settlement_reports.application.tasks.task_type import TaskType
from geh_settlement_report.settlement_reports.infrastructure.get_report_id import get_report_id_from_args
from geh_settlement_report.settlement_reports.infrastructure.spark_initializor import initialize_spark


# The start_x() methods should only have its name updated in correspondence with the
# wheels entry point for it. Further the method must remain parameterless because
# it will be called from the entry point when deployed.
def start_hourly_time_series_points() -> None:
    _start_task(TaskType.TimeSeriesHourly)


def start_quarterly_time_series_points() -> None:
    _start_task(TaskType.TimeSeriesQuarterly)


def start_metering_point_periods() -> None:
    _start_task(TaskType.MeteringPointPeriods)


def start_charge_link_periods() -> None:
    _start_task(TaskType.ChargeLinks)


def start_charge_price_points() -> None:
    _start_task(TaskType.ChargePricePoints)


def start_energy_results() -> None:
    _start_task(TaskType.EnergyResults)


def start_wholesale_results() -> None:
    _start_task(TaskType.WholesaleResults)


def start_monthly_amounts() -> None:
    _start_task(TaskType.MonthlyAmounts)


def start_zip() -> None:
    _start_task(TaskType.Zip)


def _start_task(task_type: TaskType) -> None:
    configure_logging(cloud_role_name="dbr-settlement-report", subsystem="settlement-report-aggregations")
    start_task_with_deps(task_type=task_type)


@start_trace()
def start_task_with_deps(task_type: TaskType) -> None:
    add_extras({"settlement_report_id": get_report_id_from_args(sys.argv)})  # Add extra before pydantic validation
    args = SettlementReportArgs()
    logger = Logger(__name__)
    logger.info(f"Command line arguments: {args}")
    spark = initialize_spark()
    task = task_factory.create(task_type, spark, args)
    task.execute()


def start_measurements_report() -> None:
    configure_logging(cloud_role_name="dbr-measurements-report", subsystem="measurements-report-aggregations")
    start_measurements_report_with_deps()
