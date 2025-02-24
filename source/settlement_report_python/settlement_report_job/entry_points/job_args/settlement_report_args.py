import uuid
from dataclasses import dataclass
from datetime import datetime
from typing import Optional
from pydantic import field_validator
from geh_common.application.settings import ApplicationSettings
from settlement_report_job.entry_points.job_args.calculation_type import CalculationType
from settlement_report_job.domain.utils.market_role import MarketRole


class SettlementReportArgs(ApplicationSettings):
    report_id: str
    period_start: datetime
    period_end: datetime
    calculation_type: CalculationType
    requesting_actor_market_role: MarketRole
    requesting_actor_id: str
    calculation_id_by_grid_area: dict[str, uuid.UUID] | None = None
    """ A dictionary containing grid area codes (keys) and calculation ids (values). None for balance fixing"""
    grid_area_codes: list[str] | None = None
    """ None if NOT balance fixing"""
    energy_supplier_ids: list[str] | None = None
    split_report_by_grid_area: bool = False  # implicit flag
    prevent_large_text_files: bool = False  # implicit flag
    time_zone: str = "Europe/Copenhagen"
    catalog_name: str
    settlement_reports_output_path: str | None = None

    """The path to the folder where the settlement reports are stored."""
    include_basis_data: bool = False  # implicit flag

    @field_validator("grid_area_codes")
    @classmethod
    def validate_grid_area_codes(cls, v):
        if v is None:
            return v
        if not all(
            isinstance(code, str) and code.isdigit() and 100 <= int(code) <= 999
            for code in v
        ):
            raise ValueError(
                "Grid area codes must be strings representing a three-digit number (100-999)."
            )
        return v
