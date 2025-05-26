from typing import Any

from geh_common.infrastructure.create_zip import create_zip_file
from geh_common.telemetry import use_span
from pyspark.sql import SparkSession

from geh_settlement_report.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from geh_settlement_report.entry_points.tasks.task_base import TaskBase
from geh_settlement_report.infrastructure.paths import get_report_output_path


class ZipTask(TaskBase):
    def __init__(self, spark: SparkSession, dbutils: Any, args: SettlementReportArgs) -> None:
        super().__init__(spark=spark, dbutils=dbutils, args=args)

    @use_span()
    def execute(self) -> None:
        """Entry point for the logic of creating the final zip file."""
        report_output_path = get_report_output_path(self.args)
        files_to_zip = [
            f"{report_output_path}/{file_info.name}" for file_info in self.dbutils.fs.ls(report_output_path)
        ]

        self.log.info(f"Files to zip: {files_to_zip}")
        zip_file_path = f"{self.args.settlement_reports_output_path}/{self.args.report_id}.zip"
        self.log.info(f"Creating zip file: '{zip_file_path}'")
        create_zip_file(self.dbutils, zip_file_path, files_to_zip)
        self.log.info(f"Finished creating '{zip_file_path}'")
