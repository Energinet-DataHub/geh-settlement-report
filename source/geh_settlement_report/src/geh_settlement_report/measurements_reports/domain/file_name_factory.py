from datetime import datetime
from zoneinfo import ZoneInfo

from geh_settlement_report.measurements_reports.application.job_args.measurements_report_args import (
    MeasurementsReportArgs,
)


def _convert_date(date: datetime, timezone: str):
    """Convert a date to a string in the format YYYY-MM-DD."""
    return date.astimezone(ZoneInfo(timezone)).strftime("%d-%m-%Y")


def file_name_factory(args: MeasurementsReportArgs) -> str:
    """Generate a file name based on the provided arguments."""
    return "_".join(
        [
            "measurements_report",
            "_".join(args.grid_area_codes or ["all"]),
            args.requesting_actor_id,
            _convert_date(args.period_start, args.time_zone),
            _convert_date(args.period_end, args.time_zone),
        ]
    )
