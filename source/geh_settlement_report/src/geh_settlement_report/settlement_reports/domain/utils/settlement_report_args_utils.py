from geh_settlement_report.settlement_reports.application.job_args.settlement_report_args import (
    SettlementReportArgs,
)


def should_have_result_file_per_grid_area(
    args: SettlementReportArgs,
) -> bool:
    exactly_one_grid_area_from_calc_ids = (
        args.calculation_id_by_grid_area is not None and len(args.calculation_id_by_grid_area) == 1
    )

    exactly_one_grid_area_from_grid_area_codes = args.grid_area_codes is not None and len(args.grid_area_codes) == 1

    return (
        exactly_one_grid_area_from_calc_ids
        or exactly_one_grid_area_from_grid_area_codes
        or args.split_report_by_grid_area
    )
