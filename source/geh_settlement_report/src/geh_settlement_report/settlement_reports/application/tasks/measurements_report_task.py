import shutil
import sys
from pathlib import Path

from geh_common.databricks.get_dbutils import get_dbutils
from geh_common.infrastructure.create_zip import create_zip_file
from geh_common.telemetry import Logger, use_span
from geh_common.telemetry.decorators import start_trace
from geh_common.telemetry.logging_configuration import add_extras

from geh_settlement_report.entry_points.job_args.measurements_report_args import MeasurementsReportArgs
from geh_settlement_report.infrastructure.get_report_id import get_report_id_from_args
from geh_settlement_report.infrastructure.spark_initializor import initialize_spark


@start_trace()
def start_measurements_report_with_deps():
    add_extras({"measurements_report_id": get_report_id_from_args(sys.argv)})
    args = MeasurementsReportArgs()
    spark = initialize_spark()
    dbutils = get_dbutils(spark)
    logger = Logger(__name__)
    logger.info("Starting measurements report")
    logger.info(f"Command line arguments: {args}")
    execute_measurements_report(args, dbutils, logger)


@use_span()
def execute_measurements_report(args: MeasurementsReportArgs, dbutils, logger):
    result_dir = Path(args.output_path) / args.report_id
    result_dir.mkdir(parents=True, exist_ok=True)
    files = [str(result_dir / "file1.csv"), str(result_dir / "file2.csv"), str(result_dir / "file3.csv")]
    for f in files:
        logger.info(f"Processing file: {f}")
        Path(f).write_text('a,b\n1, "a"')

    logger.info(f"Files to zip: {files}")
    zip_file = create_zip_file(dbutils, result_dir.with_suffix(".zip"), files)
    shutil.rmtree(result_dir)
    logger.info(f"Finished creating '{zip_file}'")
