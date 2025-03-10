import uuid
from datetime import datetime
from typing import Any, Annotated
from pydantic import Field, field_validator, model_validator
from pydantic_settings import BaseSettings, NoDecode, SettingsConfigDict
from settlement_report_job.entry_points.job_args.calculation_type import CalculationType

from settlement_report_job.domain.utils.market_role import MarketRole
import re


class SettlementReportArgs(BaseSettings):
    model_config = SettingsConfigDict(
        cli_parse_args=True,
        cli_kebab_case=True,
        cli_implicit_flags=True,
        cli_prog_name="settlement_report_job",
    )

    report_id: str = Field(init=False)
    period_start: datetime = Field(init=False)
    period_end: datetime = Field(init=False)
    calculation_type: CalculationType = Field(init=False)
    requesting_actor_market_role: MarketRole = Field(init=False)
    requesting_actor_id: str = Field(init=False)
    catalog_name: str = Field(init=False)
    settlement_reports_output_path: str = Field(init=False)

    calculation_id_by_grid_area: dict[str, uuid.UUID] | None = Field(
        init=False, default=None
    )
    """ A dictionary containing grid area codes (keys) and calculation ids (values). None for balance fixing"""
    grid_area_codes: Annotated[list[str], NoDecode] | None = Field(
        init=False, default=None
    )
    """ None if NOT balance fixing"""
    energy_supplier_ids: Annotated[list[str], NoDecode] | None = Field(
        init=False, default=None
    )
    time_zone: str = Field(init=False, default="Europe/Copenhagen")
    prevent_large_text_files: bool = False
    split_report_by_grid_area: bool = False
    """The path to the folder where the settlement reports are stored."""
    include_basis_data: bool = False

    @model_validator(mode="after")
    def _validate_calculation_id_by_grid_area(self) -> "SettlementReportArgs":
        if self.calculation_type == CalculationType.BALANCE_FIXING:
            if self.grid_area_codes is None:
                raise ValueError("grid_area_codes must be a list for balance fixing")
        elif self.calculation_type != CalculationType.BALANCE_FIXING:
            if self.calculation_id_by_grid_area is None:
                raise ValueError(
                    "calculation_id_by_grid_area must be a dictionary for anything but balance fixing"
                )
        return self

    @field_validator("grid_area_codes", "energy_supplier_ids", mode="before")
    @classmethod
    def _convert_grid_area_codes(cls, value: Any) -> list[str] | None:
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
        for code in v:
            assert isinstance(code, str), (
                f"Grid area codes must be strings, not {type(code)}"
            )
            if len(code) != 3 or not code.isdigit():
                raise ValueError(
                    f"Unknown grid area code: '{code}'. Grid area codes must consist of 3 digits (000-999)."
                )
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
