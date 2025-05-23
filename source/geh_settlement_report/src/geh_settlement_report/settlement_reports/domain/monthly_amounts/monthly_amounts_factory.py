from pyspark.sql import DataFrame

from geh_settlement_report.settlement_reports.application.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from geh_settlement_report.settlement_reports.domain.monthly_amounts.prepare_for_csv import (
    prepare_for_csv,
)
from geh_settlement_report.settlement_reports.domain.monthly_amounts.read_and_filter import (
    read_and_filter_from_view,
)
from geh_settlement_report.settlement_reports.domain.utils.settlement_report_args_utils import (
    should_have_result_file_per_grid_area,
)
from geh_settlement_report.settlement_reports.infrastructure.repository import WholesaleRepository


def create_monthly_amounts(
    args: SettlementReportArgs,
    repository: WholesaleRepository,
) -> DataFrame:
    monthly_amounts = read_and_filter_from_view(args, repository)

    return prepare_for_csv(
        monthly_amounts,
        should_have_result_file_per_grid_area(args=args),
    )
