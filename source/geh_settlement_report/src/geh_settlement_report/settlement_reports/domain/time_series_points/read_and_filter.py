from datetime import datetime
from uuid import UUID

from geh_common.telemetry import Logger, use_span
from pyspark.sql import DataFrame
from pyspark.sql import functions as F

from geh_settlement_report.settlement_reports.domain.time_series_points.system_operator_filter import (
    filter_time_series_points_on_charge_owner,
)
from geh_settlement_report.settlement_reports.domain.utils.factory_filters import (
    filter_by_calculation_id_by_grid_area,
    filter_by_energy_supplier_ids,
    read_and_filter_by_latest_calculations,
)
from geh_settlement_report.settlement_reports.domain.utils.market_role import MarketRole
from geh_settlement_report.settlement_reports.infrastructure.repository import WholesaleRepository
from geh_settlement_report.settlement_reports.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)
from geh_settlement_report.settlement_reports.infrastructure.wholesale.data_values import (
    MeteringPointResolutionDataProductValue,
)

log = Logger(__name__)


@use_span()
def read_and_filter_for_balance_fixing(
    period_start: datetime,
    period_end: datetime,
    grid_area_codes: list[str],
    energy_supplier_ids: list[str] | None,
    metering_point_resolution: MeteringPointResolutionDataProductValue,
    time_zone: str,
    repository: WholesaleRepository,
) -> DataFrame:
    log.info("Creating time series points")
    time_series_points = _read_from_view(
        period_start,
        period_end,
        metering_point_resolution,
        energy_supplier_ids,
        repository,
    )

    time_series_points = read_and_filter_by_latest_calculations(
        df=time_series_points,
        repository=repository,
        grid_area_codes=grid_area_codes,
        period_start=period_start,
        period_end=period_end,
        time_zone=time_zone,
        time_column_name=DataProductColumnNames.observation_time,
    )

    return time_series_points


@use_span()
def read_and_filter_for_wholesale(
    period_start: datetime,
    period_end: datetime,
    calculation_id_by_grid_area: dict[str, UUID],
    energy_supplier_ids: list[str] | None,
    metering_point_resolution: MeteringPointResolutionDataProductValue,
    requesting_actor_market_role: MarketRole,
    requesting_actor_id: str,
    repository: WholesaleRepository,
) -> DataFrame:
    log.info("Creating time series points")

    time_series_points = _read_from_view(
        period_start=period_start,
        period_end=period_end,
        resolution=metering_point_resolution,
        energy_supplier_ids=energy_supplier_ids,
        repository=repository,
    )

    time_series_points = time_series_points.where(filter_by_calculation_id_by_grid_area(calculation_id_by_grid_area))

    if requesting_actor_market_role is MarketRole.SYSTEM_OPERATOR:
        time_series_points = filter_time_series_points_on_charge_owner(
            time_series_points=time_series_points,
            system_operator_id=requesting_actor_id,
            charge_link_periods=repository.read_charge_link_periods(),
            charge_price_information_periods=repository.read_charge_price_information_periods(),
        )

    return time_series_points


@use_span()
def _read_from_view(
    period_start: datetime,
    period_end: datetime,
    resolution: MeteringPointResolutionDataProductValue,
    energy_supplier_ids: list[str] | None,
    repository: WholesaleRepository,
) -> DataFrame:
    time_series_points = repository.read_metering_point_time_series().where(
        (F.col(DataProductColumnNames.observation_time) >= period_start)
        & (F.col(DataProductColumnNames.observation_time) < period_end)
        & (F.col(DataProductColumnNames.resolution) == resolution.value)
    )

    if energy_supplier_ids:
        time_series_points = time_series_points.where(filter_by_energy_supplier_ids(energy_supplier_ids))

    return time_series_points
