from geh_common.telemetry import Logger, use_span
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, lit

from geh_settlement_report.settlement_reports.application.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from geh_settlement_report.settlement_reports.domain.utils.factory_filters import (
    filter_by_calculation_id_by_grid_area,
    filter_by_charge_owner_and_tax_depending_on_market_role,
    filter_by_energy_supplier_ids,
)
from geh_settlement_report.settlement_reports.domain.utils.market_role import MarketRole
from geh_settlement_report.settlement_reports.infrastructure.repository import WholesaleRepository
from geh_settlement_report.settlement_reports.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)

log = Logger(__name__)


@use_span()
def read_and_filter_from_view(args: SettlementReportArgs, repository: WholesaleRepository) -> DataFrame:
    monthly_amounts_per_charge = repository.read_monthly_amounts_per_charge_v1()
    monthly_amounts_per_charge = _filter_monthly_amounts_per_charge(monthly_amounts_per_charge, args)

    total_monthly_amounts = repository.read_total_monthly_amounts_v1()
    total_monthly_amounts = _filter_total_monthly_amounts(total_monthly_amounts, args)
    total_monthly_amounts = _prepare_total_monthly_amounts_columns_for_union(
        total_monthly_amounts,
        monthly_amounts_per_charge_column_ordering=monthly_amounts_per_charge.columns,
    )

    monthly_amounts = monthly_amounts_per_charge.union(total_monthly_amounts)
    monthly_amounts = monthly_amounts.withColumn(DataProductColumnNames.resolution, lit("P1M"))

    return monthly_amounts


def _apply_shared_filters(monthly_amounts: DataFrame, args: SettlementReportArgs) -> DataFrame:
    monthly_amounts = monthly_amounts.where(
        (col(DataProductColumnNames.time) >= args.period_start) & (col(DataProductColumnNames.time) < args.period_end)
    )
    if args.calculation_id_by_grid_area:
        # Can never be null, but mypy requires it be specified
        monthly_amounts = monthly_amounts.where(filter_by_calculation_id_by_grid_area(args.calculation_id_by_grid_area))
    if args.energy_supplier_ids:
        monthly_amounts = monthly_amounts.where(filter_by_energy_supplier_ids(args.energy_supplier_ids))
    return monthly_amounts


def _filter_monthly_amounts_per_charge(monthly_amounts_per_charge: DataFrame, args: SettlementReportArgs) -> DataFrame:
    monthly_amounts_per_charge = _apply_shared_filters(monthly_amounts_per_charge, args)

    monthly_amounts_per_charge = filter_by_charge_owner_and_tax_depending_on_market_role(
        monthly_amounts_per_charge,
        args.requesting_actor_market_role,
        args.requesting_actor_id,
    )

    return monthly_amounts_per_charge


def _filter_total_monthly_amounts(total_monthly_amounts: DataFrame, args: SettlementReportArgs) -> DataFrame:
    total_monthly_amounts = _apply_shared_filters(total_monthly_amounts, args)

    if args.requesting_actor_market_role in [
        MarketRole.ENERGY_SUPPLIER,
        MarketRole.DATAHUB_ADMINISTRATOR,
    ]:
        total_monthly_amounts = total_monthly_amounts.where(col(DataProductColumnNames.charge_owner_id).isNull())
    elif args.requesting_actor_market_role in [
        MarketRole.GRID_ACCESS_PROVIDER,
        MarketRole.SYSTEM_OPERATOR,
    ]:
        total_monthly_amounts = total_monthly_amounts.where(
            col(DataProductColumnNames.charge_owner_id) == args.requesting_actor_id
        )
    return total_monthly_amounts


def _prepare_total_monthly_amounts_columns_for_union(
    base_total_monthly_amounts: DataFrame,
    monthly_amounts_per_charge_column_ordering: list[str],
) -> DataFrame:
    for null_column in [
        DataProductColumnNames.quantity_unit,
        DataProductColumnNames.charge_type,
        DataProductColumnNames.charge_code,
        DataProductColumnNames.is_tax,
        DataProductColumnNames.charge_owner_id,
        # charge_owner_id is not always null, but it should not be part of total_monthly_amounts
    ]:
        base_total_monthly_amounts = base_total_monthly_amounts.withColumn(null_column, lit(None))

    return base_total_monthly_amounts.select(
        monthly_amounts_per_charge_column_ordering,
    )
