import uuid
from datetime import datetime
from typing import Any, Annotated
from pydantic import field_validator
from pydantic_settings import NoDecode
from geh_common.application.settings import ApplicationSettings
from settlement_report_job.entry_points.job_args.calculation_type import CalculationType

from settlement_report_job.domain.utils.market_role import MarketRole
import re


class SettlementReportArgs(ApplicationSettings):
    report_id: str
    period_start: datetime
    period_end: datetime
    calculation_type: CalculationType
    requesting_actor_market_role: MarketRole
    requesting_actor_id: str
    calculation_id_by_grid_area: dict[str, uuid.UUID] | None = None
    """ A dictionary containing grid area codes (keys) and calculation ids (values). None for balance fixing"""
    grid_area_codes: Annotated[list[str], NoDecode] | None = None
    """ None if NOT balance fixing"""
    energy_supplier_ids: Annotated[list[str], NoDecode] | None = None
    split_report_by_grid_area: bool = False  # implicit flag
    prevent_large_text_files: bool = False  # implicit flag
    time_zone: str = "Europe/Copenhagen"
    catalog_name: str
    settlement_reports_output_path: str | None = None

    """The path to the folder where the settlement reports are stored."""
    include_basis_data: bool = False  # implicit flag

    @field_validator("grid_area_codes", "energy_supplier_ids", mode="before")
    @classmethod
    def _validate_myvar(cls, value: Any) -> list[str] | None:
        if not value:
            return None
        if isinstance(value, list):
            return [str(item) for item in value]
        else:
            return re.findall(r"\d+", value)

    @field_validator("grid_area_codes", mode="after")
    @classmethod
    def validate_grid_area_codes(cls, v: list[str] | None) -> list[str] | None:
        if v is None:
            return v
        if not all(
            isinstance(code, str) and code.isdigit() and 100 <= int(code) <= 999
            for code in v
        ):
            raise ValueError("Grid area codes must consist of 3 digits (100-999).")
        return v

    @field_validator("energy_supplier_ids", mode="after")
    @classmethod
    def validate_energy_supplier_ids(cls, value: list[str] | None) -> list[str] | None:
        if not value:
            return None
        if any(
            (len(v) != 13 and len(v) != 16) or any(c < "0" or c > "9" for c in v)
            for v in value
        ):
            msg = "Energy supplier IDs must consist of 13 or 16 digits"
            raise ValueError(msg)
        return value
