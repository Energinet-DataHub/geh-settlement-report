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


import shutil
import sys
from pathlib import Path

from geh_common.databricks.get_dbutils import get_dbutils
from geh_common.infrastructure.create_zip import create_zip_file
from geh_common.telemetry.decorators import start_trace
from geh_common.telemetry.logger import Logger
from geh_common.telemetry.logging_configuration import (
    add_extras,
    configure_logging,
)

from geh_settlement_report.entry_points.job_args.measurements_report_args import MeasurementsReportArgs
from geh_settlement_report.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from geh_settlement_report.entry_points.tasks import task_factory
from geh_settlement_report.entry_points.tasks.task_type import TaskType
from geh_settlement_report.infrastructure.spark_initializor import initialize_spark


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


def get_report_id_from_args(args: list[str] = sys.argv) -> str:
    """Check if --report-id is part of sys.argv and returns its value.

    Returns:
        str: The value of --report-id

    Raises:
        ValueError: If --report-id is not found in sys.argv
    """
    for i, arg in enumerate(args):
        if arg.startswith("--report-id"):
            if "=" in arg:
                return arg.split("=")[1]
            else:
                if i + 1 <= len(args):
                    return args[i + 1]
    raise ValueError(f"'--report-id' was not found in arguments. Existing arguments: {','.join(sys.argv)}")


def start_measurements_report() -> None:
    args = MeasurementsReportArgs()
    spark = initialize_spark()
    logger = Logger(__name__)
    logger.info("Starting measurements report")
    result_dir = Path(args.output_path) / args.report_id
    result_dir.mkdir(parents=True, exist_ok=True)
    tmpdir = Path("tmp")
    files = [str(result_dir / "file1.csv"), str(result_dir / "file2.csv"), str(result_dir / "file3.csv")]
    for f in files:
        logger.info(f"Processing file: {f}")
        Path(f).write_text('a,b\n1, "a"')
    logger.info(f"Files to zip: {files}")
    dbutils = get_dbutils(spark)
    zip_file = create_zip_file(dbutils, result_dir, files)
    shutil.rmtree(result_dir)
    shutil.rmtree(tmpdir)
    logger.info(f"Finished creating '{zip_file}'")
