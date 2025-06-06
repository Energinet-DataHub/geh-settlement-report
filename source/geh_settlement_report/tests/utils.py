import os
import shutil
from datetime import datetime, timedelta, timezone
from zoneinfo import ZoneInfo

from geh_settlement_report.settlement_reports.application.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from geh_settlement_report.settlement_reports.domain.utils.market_role import MarketRole
from geh_settlement_report.settlement_reports.domain.utils.report_data_type import ReportDataType
from geh_settlement_report.settlement_reports.infrastructure import paths
from geh_settlement_report.settlement_reports.infrastructure.report_name_factory import (
    MarketRoleInFileName,
)


class Dates:
    JAN_1ST = datetime(2023, 12, 31, 23, tzinfo=timezone.utc)
    JAN_2ND = datetime(2024, 1, 1, 23, tzinfo=timezone.utc)
    JAN_3RD = datetime(2024, 1, 2, 23, tzinfo=timezone.utc)
    JAN_4TH = datetime(2024, 1, 3, 23, tzinfo=timezone.utc)
    JAN_5TH = datetime(2024, 1, 4, 23, tzinfo=timezone.utc)
    JAN_6TH = datetime(2024, 1, 5, 23, tzinfo=timezone.utc)
    JAN_7TH = datetime(2024, 1, 6, 23, tzinfo=timezone.utc)
    JAN_8TH = datetime(2024, 1, 7, 23, tzinfo=timezone.utc)
    JAN_9TH = datetime(2024, 1, 8, 23, tzinfo=timezone.utc)


DEFAULT_TIME_ZONE = "Europe/Copenhagen"


def cleanup_output_path(settlement_reports_output_path: str) -> None:
    if os.path.exists(settlement_reports_output_path):
        shutil.rmtree(settlement_reports_output_path)
        os.makedirs(settlement_reports_output_path)


def get_actual_files(report_data_type: ReportDataType, args: SettlementReportArgs) -> list[str]:
    path = paths.get_report_output_path(args)
    if not os.path.isdir(path):
        return []

    return [
        f
        for f in os.listdir(path)
        if os.path.isfile(os.path.join(path, f))
        and f.startswith(_get_file_prefix(report_data_type))
        and f.endswith(".csv")
    ]


def _get_file_prefix(report_data_type) -> str:
    if report_data_type == ReportDataType.TimeSeriesHourly:
        return "TSSD60"
    elif report_data_type == ReportDataType.TimeSeriesQuarterly:
        return "TSSD15"
    elif report_data_type == ReportDataType.MeteringPointPeriods:
        return "MDMP"
    elif report_data_type == ReportDataType.ChargeLinks:
        return "CHARGELINK"
    elif report_data_type == ReportDataType.ChargePricePoints:
        return "CHARGEPRICE"
    elif report_data_type == ReportDataType.EnergyResults:
        return "RESULTENERGY"
    elif report_data_type == ReportDataType.WholesaleResults:
        return "RESULTWHOLESALE"
    elif report_data_type == ReportDataType.MonthlyAmounts:
        return "RESULTMONTHLY"
    raise NotImplementedError(f"Report data type {report_data_type} is not supported.")


def get_start_date(period_start: datetime) -> str:
    time_zone_info = ZoneInfo(DEFAULT_TIME_ZONE)
    return period_start.astimezone(time_zone_info).strftime("%d-%m-%Y")


def get_end_date(period_end: datetime) -> str:
    time_zone_info = ZoneInfo(DEFAULT_TIME_ZONE)
    return (period_end.astimezone(time_zone_info) - timedelta(days=1)).strftime("%d-%m-%Y")


def get_market_role_in_file_name(
    requesting_actor_market_role: MarketRole,
) -> str | None:
    if requesting_actor_market_role == MarketRole.ENERGY_SUPPLIER:
        return MarketRoleInFileName.ENERGY_SUPPLIER
    elif requesting_actor_market_role == MarketRole.GRID_ACCESS_PROVIDER:
        return MarketRoleInFileName.GRID_ACCESS_PROVIDER

    return None
