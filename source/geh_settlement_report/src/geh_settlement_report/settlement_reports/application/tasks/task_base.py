from abc import abstractmethod
from typing import Any

from geh_common.telemetry import Logger
from pyspark.sql import SparkSession

from geh_settlement_report.settlement_reports.application.job_args.settlement_report_args import (
    SettlementReportArgs,
)


class TaskBase:
    def __init__(self, spark: SparkSession, dbutils: Any, args: SettlementReportArgs) -> None:
        self.spark = spark
        self.dbutils = dbutils
        self.args = args
        self.log = Logger(__name__)

    @abstractmethod
    def execute(self) -> None:
        pass
