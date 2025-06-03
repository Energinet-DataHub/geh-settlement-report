import shutil
import sys
from pathlib import Path

from geh_common.telemetry import Logger, use_span
from geh_common.telemetry.decorators import start_trace
from geh_common.telemetry.logging_configuration import add_extras
from pyspark.sql import SparkSession

from geh_settlement_report.common.get_report_id import get_report_id_from_args
from geh_settlement_report.common.infrastructure.current_measurements_repository import CurrentMeasurementsRepository
from geh_settlement_report.common.infrastructure.electricity_market_respository import ElectricityMarketRepository
from geh_settlement_report.common.spark_initializor import initialize_spark
from geh_settlement_report.measurements_reports.application.job_args.measurements_report_args import (
    MeasurementsReportArgs,
)
from geh_settlement_report.measurements_reports.domain.calculation import execute


@start_trace()
def start_measurements_report_with_deps():
    add_extras({"measurements_report_id": get_report_id_from_args(sys.argv)})
    args = MeasurementsReportArgs()
    spark = initialize_spark()
    logger = Logger(__name__)
    logger.info("Starting measurements report")
    logger.info(f"Command line arguments: {args}")
    execute_measurements_report(args, spark, logger)


@use_span()
def execute_measurements_report(args: MeasurementsReportArgs, spark: SparkSession, logger) -> None:
    logger.info("Creating temporary directory for report output before zipping")
    result_dir = Path(args.output_path) / args.report_id
    result_dir.mkdir(parents=True, exist_ok=True)

    current_measurements_repository = CurrentMeasurementsRepository(spark, args.catalog_name)
    electricity_market_repository = ElectricityMarketRepository(spark, args.catalog_name)
    logger.info("Reading input data")
    current_measurements = current_measurements_repository.read_current_measurements()
    metering_point_periods = electricity_market_repository.read_measurements_report_metering_point_periods()

    logger.info("Zipping files into report")
    execute(
        spark,
        args,
        current_measurements.df,
        metering_point_periods.df,
    )

    logger.info("Removing the temporary folder")
    shutil.rmtree(Path(args.output_path) / args.report_id)
