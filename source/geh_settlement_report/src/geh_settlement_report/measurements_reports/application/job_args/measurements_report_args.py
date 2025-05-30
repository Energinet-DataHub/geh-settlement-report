import re
from datetime import datetime
from typing import Annotated, Any

from geh_common.application import GridAreaCodes
from pydantic import AfterValidator, BeforeValidator, Field
from pydantic_settings import BaseSettings, NoDecode, SettingsConfigDict


def _str_to_list(value: Any) -> list[str] | None:
    """Convert the input value to a list of grid area codes (strings).

    Args:
        value (Any): The input value to convert.

    Returns:
        Optional[List[str]]: A list of grid area codes or None if the input is empty.
    """
    if not value:
        return None
    if isinstance(value, list):
        return [str(item) for item in value]
    else:
        return re.findall(r"\d+", value)


def _validate_energy_supplier_ids(v: list[str]) -> list[str]:
    """Validate the list of energy supplier IDs."""
    if not v:
        return v
    for id_ in v:
        if not isinstance(id_, str):
            raise TypeError(f"Energy supplier IDs must be strings, not {type(id_)}")
        if not id_.isdigit():
            raise ValueError(f"Unexpected energy supplier ID: '{id_}'.")
    return v


EnergySupplierIds = Annotated[
    list[str] | None, BeforeValidator(_str_to_list), AfterValidator(_validate_energy_supplier_ids), NoDecode()
]


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
