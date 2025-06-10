from typing import Any

from geh_common.telemetry import use_span
from pyspark.sql import SparkSession

from geh_settlement_report.settlement_reports.application.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from geh_settlement_report.settlement_reports.application.tasks.task_base import (
    TaskBase,
)
from geh_settlement_report.settlement_reports.domain.charge_link_periods.charge_link_periods_factory import (
    create_charge_link_periods,
)
from geh_settlement_report.settlement_reports.domain.charge_link_periods.order_by_columns import (
    order_by_columns,
)
from geh_settlement_report.settlement_reports.domain.utils.report_data_type import ReportDataType
from geh_settlement_report.settlement_reports.infrastructure import csv_writer
from geh_settlement_report.settlement_reports.infrastructure.repository import WholesaleRepository


class ChargeLinkPeriodsTask(TaskBase):
    def __init__(self, spark: SparkSession, dbutils: Any, args: SettlementReportArgs) -> None:
        super().__init__(spark=spark, dbutils=dbutils, args=args)

    @use_span()
    def execute(self) -> None:
        """Entry point for the logic of creating charge links."""
        if not self.args.include_basis_data:
            return

        repository = WholesaleRepository(self.spark, self.args.catalog_name)
        charge_link_periods = create_charge_link_periods(args=self.args, repository=repository)

        csv_writer.write(
            args=self.args,
            spark=self.spark,
            df=charge_link_periods,
            report_data_type=ReportDataType.ChargeLinks,
            order_by_columns=order_by_columns(self.args.requesting_actor_market_role),
        )
