from pyspark.sql import DataFrame

from geh_settlement_report.settlement_reports.application.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from geh_settlement_report.settlement_reports.domain.energy_results.prepare_for_csv import (
    prepare_for_csv,
)
from geh_settlement_report.settlement_reports.domain.energy_results.read_and_filter import (
    read_and_filter_from_view,
)
from geh_settlement_report.settlement_reports.domain.utils.settlement_report_args_utils import (
    should_have_result_file_per_grid_area,
)
from geh_settlement_report.settlement_reports.infrastructure.repository import WholesaleRepository


def create_energy_results(
    args: SettlementReportArgs,
    repository: WholesaleRepository,
) -> DataFrame:
    energy = read_and_filter_from_view(args, repository)

    return prepare_for_csv(
        energy,
        should_have_result_file_per_grid_area(args),
        args.requesting_actor_market_role,
    )
