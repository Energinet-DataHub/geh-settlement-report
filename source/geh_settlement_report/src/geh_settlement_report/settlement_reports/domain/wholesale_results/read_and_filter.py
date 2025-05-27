from datetime import datetime
from uuid import UUID

from geh_common.telemetry import Logger, use_span
from pyspark.sql import DataFrame
from pyspark.sql import functions as F

from geh_settlement_report.settlement_reports.domain.utils.factory_filters import (
    filter_by_calculation_id_by_grid_area,
    filter_by_charge_owner_and_tax_depending_on_market_role,
)
from geh_settlement_report.settlement_reports.domain.utils.market_role import MarketRole
from geh_settlement_report.settlement_reports.infrastructure.repository import WholesaleRepository
from geh_settlement_report.settlement_reports.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)

log = Logger(__name__)


@use_span()
def read_and_filter_from_view(
    energy_supplier_ids: list[str] | None,
    calculation_id_by_grid_area: dict[str, UUID],
    period_start: datetime,
    period_end: datetime,
    requesting_actor_market_role: MarketRole,
    requesting_actor_id: str,
    repository: WholesaleRepository,
) -> DataFrame:
    df = repository.read_amounts_per_charge().where(
        (F.col(DataProductColumnNames.time) >= period_start) & (F.col(DataProductColumnNames.time) < period_end)
    )

    if energy_supplier_ids is not None:
        df = df.where(F.col(DataProductColumnNames.energy_supplier_id).isin(energy_supplier_ids))

    df = df.where(filter_by_calculation_id_by_grid_area(calculation_id_by_grid_area))

    df = filter_by_charge_owner_and_tax_depending_on_market_role(df, requesting_actor_market_role, requesting_actor_id)

    return df
