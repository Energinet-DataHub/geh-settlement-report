from typing import Any

from geh_common.telemetry import use_span
from pyspark.sql import SparkSession

from geh_settlement_report.settlement_reports.application.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from geh_settlement_report.settlement_reports.application.tasks.task_base import (
    TaskBase,
)
from geh_settlement_report.settlement_reports.domain.charge_price_points.charge_price_points_factory import (
    create_charge_price_points,
)
from geh_settlement_report.settlement_reports.domain.charge_price_points.order_by_columns import (
    order_by_columns,
)
from geh_settlement_report.settlement_reports.domain.utils.report_data_type import ReportDataType
from geh_settlement_report.settlement_reports.infrastructure import csv_writer
from geh_settlement_report.settlement_reports.infrastructure.repository import WholesaleRepository


class ChargePricePointsTask(TaskBase):
    def __init__(self, spark: SparkSession, dbutils: Any, args: SettlementReportArgs) -> None:
        super().__init__(spark=spark, dbutils=dbutils, args=args)

    @use_span()
    def execute(self) -> None:
        """Entry point for the logic of creating charge prices."""
        if not self.args.include_basis_data:
            return

        repository = WholesaleRepository(self.spark, self.args.catalog_name)
        charge_price_points = create_charge_price_points(args=self.args, repository=repository)

        csv_writer.write(
            args=self.args,
            df=charge_price_points,
            report_data_type=ReportDataType.ChargePricePoints,
            order_by_columns=order_by_columns(),
        )
