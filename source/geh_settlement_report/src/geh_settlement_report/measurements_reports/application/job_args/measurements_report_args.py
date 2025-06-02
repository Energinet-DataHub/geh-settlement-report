from datetime import datetime

from geh_common.application import EnergySupplierIds, GridAreaCodes
from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict


class MeasurementsReportArgs(BaseSettings):
    model_config = SettingsConfigDict(
        cli_parse_args=True,
        cli_kebab_case=True,
        cli_implicit_flags=True,
        cli_ignore_unknown_args=True,
        cli_prog_name="measurements_report_job",
    )

    report_id: str = Field(init=False)
    period_start: datetime = Field(init=False)
    period_end: datetime = Field(init=False)
    grid_area_codes: GridAreaCodes | None = Field(init=False, default=None)
    requesting_actor_id: str = Field(init=False)
    # TODO BJM: Update to correctly parse input parameter - share functionality with settlement report args
    energy_supplier_ids: EnergySupplierIds = Field(init=False, default_factory=list)

    catalog_name: str = Field(init=False)
    output_path: str = Field(init=False)
    time_zone: str = Field(init=False, default="Europe/Copenhagen")
