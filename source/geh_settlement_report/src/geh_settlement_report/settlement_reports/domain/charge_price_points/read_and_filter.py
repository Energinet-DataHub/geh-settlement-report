from datetime import datetime
from uuid import UUID

from geh_common.telemetry import Logger, use_span
from pyspark.sql import DataFrame
from pyspark.sql import functions as F

from geh_settlement_report.settlement_reports.domain.utils.factory_filters import (
    filter_by_charge_owner_and_tax_depending_on_market_role,
)
from geh_settlement_report.settlement_reports.domain.utils.join_metering_points_periods_and_charge_link_periods import (
    join_metering_points_periods_and_charge_link_periods,
)
from geh_settlement_report.settlement_reports.domain.utils.market_role import MarketRole
from geh_settlement_report.settlement_reports.domain.utils.repository_filtering import (
    read_metering_point_periods_by_calculation_ids,
)
from geh_settlement_report.settlement_reports.infrastructure.repository import WholesaleRepository
from geh_settlement_report.settlement_reports.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)

logger = Logger(__name__)


@use_span()
def read_and_filter(
    period_start: datetime,
    period_end: datetime,
    calculation_id_by_grid_area: dict[str, UUID],
    energy_supplier_ids: list[str] | None,
    requesting_actor_market_role: MarketRole,
    requesting_actor_id: str,
    repository: WholesaleRepository,
) -> DataFrame:
    logger.info("Creating charge prices")

    charge_price_points = (
        repository.read_charge_price_points()
        .where(F.col(DataProductColumnNames.charge_time) >= period_start)
        .where(F.col(DataProductColumnNames.charge_time) < period_end)
    )

    charge_price_points = _join_with_charge_link_and_metering_point_periods(
        charge_price_points,
        period_start,
        period_end,
        calculation_id_by_grid_area,
        energy_supplier_ids,
        repository,
    )

    charge_price_information_periods = repository.read_charge_price_information_periods()

    charge_price_points = charge_price_points.join(
        charge_price_information_periods,
        on=[
            DataProductColumnNames.calculation_id,
            DataProductColumnNames.charge_key,
        ],
        how="inner",
    ).select(
        charge_price_points["*"],
        charge_price_information_periods[DataProductColumnNames.is_tax],
        charge_price_information_periods[DataProductColumnNames.resolution],
    )

    if requesting_actor_market_role in [
        MarketRole.SYSTEM_OPERATOR,
        MarketRole.GRID_ACCESS_PROVIDER,
    ]:
        charge_price_points = filter_by_charge_owner_and_tax_depending_on_market_role(
            charge_price_points,
            requesting_actor_market_role,
            requesting_actor_id,
        )

    return charge_price_points


def _join_with_charge_link_and_metering_point_periods(
    charge_price_points: DataFrame,
    period_start: datetime,
    period_end: datetime,
    calculation_id_by_grid_area: dict[str, UUID],
    energy_supplier_ids: list[str] | None,
    repository: WholesaleRepository,
) -> DataFrame:
    charge_link_periods = repository.read_charge_link_periods().where(
        (F.col(DataProductColumnNames.from_date) < period_end) & (F.col(DataProductColumnNames.to_date) > period_start)
    )

    metering_point_periods = read_metering_point_periods_by_calculation_ids(
        repository=repository,
        period_start=period_start,
        period_end=period_end,
        calculation_id_by_grid_area=calculation_id_by_grid_area,
        energy_supplier_ids=energy_supplier_ids,
    )

    charge_link_periods_and_metering_point_periods = join_metering_points_periods_and_charge_link_periods(
        charge_link_periods, metering_point_periods
    )

    charge_price_points = (
        charge_price_points.join(
            charge_link_periods_and_metering_point_periods,
            on=[
                DataProductColumnNames.calculation_id,
                DataProductColumnNames.charge_key,
            ],
            how="inner",
        )
        .where(F.col(DataProductColumnNames.charge_time) >= F.col(DataProductColumnNames.from_date))
        .where(F.col(DataProductColumnNames.charge_time) < F.col(DataProductColumnNames.to_date))
        .select(
            charge_price_points["*"],
            charge_link_periods_and_metering_point_periods[DataProductColumnNames.grid_area_code],
        )
    ).distinct()

    return charge_price_points
