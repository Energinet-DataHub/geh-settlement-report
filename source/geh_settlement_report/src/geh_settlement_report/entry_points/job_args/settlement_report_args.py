import re
import uuid
from datetime import datetime
from typing import Annotated, Any

from geh_common.application.settings import ApplicationSettings
from pydantic import Field, field_validator
from pydantic_settings import NoDecode

from geh_settlement_report.domain.utils.market_role import MarketRole
from geh_settlement_report.entry_points.job_args.calculation_type import CalculationType


class SettlementReportArgs(ApplicationSettings):
    report_id: str = Field(init=False)
    period_start: datetime = Field(init=False)
    period_end: datetime = Field(init=False)
    calculation_type: CalculationType = Field(init=False)
    requesting_actor_market_role: MarketRole = Field(init=False)
    requesting_actor_id: str = Field(init=False)
    calculation_id_by_grid_area: dict[str, uuid.UUID] | None = Field(init=False, default=None)
    """ A dictionary containing grid area codes (keys) and calculation ids (values). None for balance fixing"""
    grid_area_codes: Annotated[list[str], NoDecode] | None = Field(init=False, default=None)
    """ None if NOT balance fixing"""
    energy_supplier_ids: Annotated[list[str], NoDecode] | None = Field(init=False, default=None)
    split_report_by_grid_area: bool = Field(init=False, default=False)  # implicit flag
    prevent_large_text_files: bool = Field(init=False, default=False)  # implicit flag
    time_zone: str = Field(init=False, default="Europe/Copenhagen")
    catalog_name: str = Field(init=False)
    settlement_reports_output_path: str | None = Field(default=None)

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
        if not all(isinstance(code, str) and code.isdigit() and 100 <= int(code) <= 999 for code in v):
            raise ValueError("Grid area codes must consist of 3 digits (100-999).")
        return v

    @field_validator("energy_supplier_ids", mode="after")
    @classmethod
    def validate_energy_supplier_ids(cls, value: list[str] | None) -> list[str] | None:
        if not value:
            return None
        if any((len(v) != 13 and len(v) != 16) or any(c < "0" or c > "9" for c in v) for v in value):
            msg = "Energy supplier IDs must consist of 13 or 16 digits"
            raise ValueError(msg)
        return value
