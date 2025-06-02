import uuid
from datetime import datetime

from geh_common.application import EnergySupplierIds, GridAreaCodes
from pydantic import Field, model_validator
from pydantic_settings import BaseSettings, SettingsConfigDict

from geh_settlement_report.settlement_reports.application.job_args.calculation_type import CalculationType
from geh_settlement_report.settlement_reports.domain.utils.market_role import MarketRole


class SettlementReportArgs(BaseSettings):
    model_config = SettingsConfigDict(
        cli_parse_args=True,
        cli_kebab_case=True,
        cli_implicit_flags=True,
        cli_ignore_unknown_args=True,
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

    calculation_id_by_grid_area: dict[str, uuid.UUID] | None = Field(init=False, default=None)
    """ A dictionary containing grid area codes (keys) and calculation ids (values). None for balance fixing"""
    grid_area_codes: GridAreaCodes | None = Field(init=False, default=None)
    """ None if NOT balance fixing"""
    energy_supplier_ids: EnergySupplierIds | None = Field(init=False, default=None)
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
                raise ValueError("calculation_id_by_grid_area must be a dictionary for anything but balance fixing")
        return self
