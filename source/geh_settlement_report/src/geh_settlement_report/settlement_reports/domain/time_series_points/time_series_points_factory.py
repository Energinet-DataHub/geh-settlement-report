from datetime import datetime
from uuid import UUID

from geh_common.telemetry import Logger, use_span
from pyspark.sql import DataFrame

from geh_settlement_report.settlement_reports.domain.time_series_points.prepare_for_csv import (
    prepare_for_csv,
)
from geh_settlement_report.settlement_reports.domain.time_series_points.read_and_filter import (
    read_and_filter_for_balance_fixing,
    read_and_filter_for_wholesale,
)
from geh_settlement_report.settlement_reports.domain.utils.market_role import MarketRole
from geh_settlement_report.settlement_reports.infrastructure.repository import WholesaleRepository
from geh_settlement_report.settlement_reports.infrastructure.wholesale.data_values import (
    MeteringPointResolutionDataProductValue,
)

log = Logger(__name__)


@use_span()
def create_time_series_points_for_balance_fixing(
    period_start: datetime,
    period_end: datetime,
    grid_area_codes: list[str],
    energy_supplier_ids: list[str] | None,
    metering_point_resolution: MeteringPointResolutionDataProductValue,
    time_zone: str,
    requesting_market_role: MarketRole,
    repository: WholesaleRepository,
) -> DataFrame:
    log.info("Creating time series points")

    time_series_points = read_and_filter_for_balance_fixing(
        period_start=period_start,
        period_end=period_end,
        grid_area_codes=grid_area_codes,
        energy_supplier_ids=energy_supplier_ids,
        metering_point_resolution=metering_point_resolution,
        time_zone=time_zone,
        repository=repository,
    )

    prepared_time_series_points = prepare_for_csv(
        filtered_time_series_points=time_series_points,
        metering_point_resolution=metering_point_resolution,
        time_zone=time_zone,
        requesting_actor_market_role=requesting_market_role,
    )
    return prepared_time_series_points


@use_span()
def create_time_series_points_for_wholesale(
    period_start: datetime,
    period_end: datetime,
    calculation_id_by_grid_area: dict[str, UUID],
    energy_supplier_ids: list[str] | None,
    metering_point_resolution: MeteringPointResolutionDataProductValue,
    requesting_actor_market_role: MarketRole,
    requesting_actor_id: str,
    time_zone: str,
    repository: WholesaleRepository,
) -> DataFrame:
    log.info("Creating time series points")

    time_series_points = read_and_filter_for_wholesale(
        period_start=period_start,
        period_end=period_end,
        calculation_id_by_grid_area=calculation_id_by_grid_area,
        energy_supplier_ids=energy_supplier_ids,
        metering_point_resolution=metering_point_resolution,
        requesting_actor_market_role=requesting_actor_market_role,
        requesting_actor_id=requesting_actor_id,
        repository=repository,
    )

    prepared_time_series_points = prepare_for_csv(
        filtered_time_series_points=time_series_points,
        metering_point_resolution=metering_point_resolution,
        time_zone=time_zone,
        requesting_actor_market_role=requesting_actor_market_role,
    )
    return prepared_time_series_points
