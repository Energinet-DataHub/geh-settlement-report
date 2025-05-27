from pyspark.sql import DataFrame

from geh_settlement_report.settlement_reports.application.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from geh_settlement_report.settlement_reports.domain.charge_link_periods.prepare_for_csv import (
    prepare_for_csv,
)
from geh_settlement_report.settlement_reports.domain.charge_link_periods.read_and_filter import (
    read_and_filter,
)
from geh_settlement_report.settlement_reports.infrastructure.repository import WholesaleRepository


def create_charge_link_periods(
    args: SettlementReportArgs,
    repository: WholesaleRepository,
) -> DataFrame:
    charge_link_periods = read_and_filter(
        args.period_start,
        args.period_end,
        args.calculation_id_by_grid_area,
        args.energy_supplier_ids,
        args.requesting_actor_market_role,
        args.requesting_actor_id,
        repository,
    )

    return prepare_for_csv(
        charge_link_periods,
    )
