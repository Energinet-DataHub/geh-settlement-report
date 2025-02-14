import uuid
from dataclasses import dataclass
from datetime import datetime
from typing import Optional
from geh_common.parsing.pydantic_settings_parsing import PydanticParsingSettings
from settlement_report_job.entry_points.job_args.calculation_type import CalculationType
from settlement_report_job.domain.utils.market_role import MarketRole


# @dataclass
class SettlementReportArgs(PydanticParsingSettings):
    report_id: str
    period_start: datetime
    period_end: datetime
    calculation_type: CalculationType
    requesting_actor_market_role: MarketRole
    requesting_actor_id: str
    calculation_id_by_grid_area: dict[str, uuid.UUID] | None = None
    """ A dictionary containing grid area codes (keys) and calculation ids (values). None for balance fixing"""
    grid_area_codes: list[int] | None = None  # TODO: actually int
    """ None if NOT balance fixing"""
    energy_supplier_ids: list[int] | None = None  # TODO: actually int
    split_report_by_grid_area: bool
    prevent_large_text_files: bool
    time_zone: str = "Europe/Copenhagen"  # TODO: Introduced a default
    catalog_name: str
    settlement_reports_output_path: str | None = (
        None  # TODO: not required per parameters-reference.txt
    )

    """The path to the folder where the settlement reports are stored."""
    include_basis_data: bool


@dataclass
class SettlementReportArgs_old:
    report_id: str
    period_start: datetime
    period_end: datetime
    calculation_type: CalculationType
    requesting_actor_market_role: MarketRole
    requesting_actor_id: str
    calculation_id_by_grid_area: Optional[dict[str, uuid.UUID]]
    """ A dictionary containing grid area codes (keys) and calculation ids (values). None for balance fixing"""
    grid_area_codes: Optional[list[str]]
    """ None if NOT balance fixing"""
    energy_supplier_ids: Optional[list[str]]
    split_report_by_grid_area: bool
    prevent_large_text_files: bool
    time_zone: str
    catalog_name: str
    settlement_reports_output_path: str
    """The path to the folder where the settlement reports are stored."""
    include_basis_data: bool
