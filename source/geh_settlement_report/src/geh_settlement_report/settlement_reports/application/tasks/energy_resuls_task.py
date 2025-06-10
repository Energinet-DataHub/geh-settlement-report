from typing import Any

from geh_common.telemetry import use_span
from pyspark.sql import SparkSession

from geh_settlement_report.settlement_reports.application.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from geh_settlement_report.settlement_reports.application.tasks.task_base import TaskBase
from geh_settlement_report.settlement_reports.domain.energy_results.energy_results_factory import (
    create_energy_results,
)
from geh_settlement_report.settlement_reports.domain.energy_results.order_by_columns import (
    order_by_columns,
)
from geh_settlement_report.settlement_reports.domain.utils.market_role import MarketRole
from geh_settlement_report.settlement_reports.domain.utils.report_data_type import ReportDataType
from geh_settlement_report.settlement_reports.infrastructure import csv_writer
from geh_settlement_report.settlement_reports.infrastructure.repository import WholesaleRepository


class EnergyResultsTask(TaskBase):
    def __init__(self, spark: SparkSession, dbutils: Any, args: SettlementReportArgs) -> None:
        super().__init__(spark=spark, dbutils=dbutils, args=args)

    @use_span()
    def execute(self) -> None:
        """Entry point for the logic of creating energy results."""
        if self.args.requesting_actor_market_role == MarketRole.SYSTEM_OPERATOR:
            return

        repository = WholesaleRepository(self.spark, self.args.catalog_name)
        energy_results_df = create_energy_results(args=self.args, repository=repository)

        csv_writer.write(
            args=self.args,
            spark=self.spark,
            df=energy_results_df,
            report_data_type=ReportDataType.EnergyResults,
            order_by_columns=order_by_columns(self.args.requesting_actor_market_role),
        )
