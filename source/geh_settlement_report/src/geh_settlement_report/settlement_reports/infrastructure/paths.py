from geh_settlement_report.settlement_reports.application.job_args.settlement_report_args import (
    SettlementReportArgs,
)


def get_settlement_reports_output_path(catalog_name: str) -> str:
    return f"/Volumes/{catalog_name}/settlement_report_output/settlement_reports"  # noqa: E501


def get_report_output_path(args: SettlementReportArgs) -> str:
    return f"{args.settlement_reports_output_path}/{args.report_id}"
