from typing import Any

from geh_common.telemetry import use_span
from pyspark.sql import SparkSession

from geh_settlement_report.settlement_reports.application.job_args.calculation_type import CalculationType
from geh_settlement_report.settlement_reports.application.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from geh_settlement_report.settlement_reports.application.tasks.task_base import TaskBase
from geh_settlement_report.settlement_reports.application.tasks.task_type import TaskType
from geh_settlement_report.settlement_reports.domain.time_series_points.order_by_columns import (
    order_by_columns,
)
from geh_settlement_report.settlement_reports.domain.time_series_points.time_series_points_factory import (
    create_time_series_points_for_balance_fixing,
    create_time_series_points_for_wholesale,
)
from geh_settlement_report.settlement_reports.domain.utils.report_data_type import ReportDataType
from geh_settlement_report.settlement_reports.infrastructure import csv_writer
from geh_settlement_report.settlement_reports.infrastructure.repository import WholesaleRepository
from geh_settlement_report.settlement_reports.infrastructure.wholesale.data_values import (
    MeteringPointResolutionDataProductValue,
)


class TimeSeriesPointsTask(TaskBase):
    def __init__(
        self,
        spark: SparkSession,
        dbutils: Any,
        args: SettlementReportArgs,
        task_type: TaskType,
    ) -> None:
        super().__init__(spark=spark, dbutils=dbutils, args=args)
        self.task_type = task_type

    @use_span()
    def execute(
        self,
    ) -> None:
        """Entry point for the logic of creating time series."""
        if not self.args.include_basis_data:
            return

        if self.task_type is TaskType.TimeSeriesHourly:
            report_type = ReportDataType.TimeSeriesHourly
            metering_point_resolution = MeteringPointResolutionDataProductValue.HOUR
        elif self.task_type is TaskType.TimeSeriesQuarterly:
            report_type = ReportDataType.TimeSeriesQuarterly
            metering_point_resolution = MeteringPointResolutionDataProductValue.QUARTER
        else:
            raise ValueError(f"Unsupported report data type: {self.task_type}")

        repository = WholesaleRepository(self.spark, self.args.catalog_name)
        if self.args.calculation_type is CalculationType.BALANCE_FIXING:
            time_series_points_df = create_time_series_points_for_balance_fixing(
                period_start=self.args.period_start,
                period_end=self.args.period_end,
                grid_area_codes=self.args.grid_area_codes,
                time_zone=self.args.time_zone,
                energy_supplier_ids=self.args.energy_supplier_ids,
                metering_point_resolution=metering_point_resolution,
                requesting_market_role=self.args.requesting_actor_market_role,
                repository=repository,
            )
        else:
            time_series_points_df = create_time_series_points_for_wholesale(
                period_start=self.args.period_start,
                period_end=self.args.period_end,
                calculation_id_by_grid_area=self.args.calculation_id_by_grid_area,
                time_zone=self.args.time_zone,
                energy_supplier_ids=self.args.energy_supplier_ids,
                metering_point_resolution=metering_point_resolution,
                repository=repository,
                requesting_actor_market_role=self.args.requesting_actor_market_role,
                requesting_actor_id=self.args.requesting_actor_id,
            )

        csv_writer.write(
            args=self.args,
            df=time_series_points_df,
            report_data_type=report_type,
            order_by_columns=order_by_columns(self.args.requesting_actor_market_role),
        )
