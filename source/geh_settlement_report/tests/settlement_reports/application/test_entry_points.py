import importlib.metadata
from typing import Any

import pytest

from geh_settlement_report.settlement_reports import entry_point as module
from geh_settlement_report.settlement_reports.entry_point import (
    get_report_id_from_args,
)


def _assert_entry_point_exists(entry_point_name: str) -> Any:
    # Load the entry point function from the installed wheel
    try:
        entry_points = importlib.metadata.entry_points(group="console_scripts")
        entry_point_names = [ep.name for ep in entry_points]

        if not entry_points:
            assert False, (
                f"The {entry_point_name} entry point was not found. Available entry points: {entry_point_names}"
            )

        # Filter for the specific entry point group and name
        matching_entry_points = [ep for ep in entry_points if ep.name == entry_point_name]

        if entry_point_name not in entry_point_names:
            assert False, (
                f"The {entry_point_name} entry point was not found. Available entry points: {entry_point_names}"
            )

        # Check if the module exists
        entry_point = matching_entry_points[0]  # Get the single entry point
        function_name = entry_point.value.split(":")[1]
        full_module_path = entry_point.value.split(":")[0]

        if not hasattr(
            module,
            function_name,
        ):
            assert False, f"The entry point module function {function_name} does not exist in entry_point.py."

        importlib.import_module(full_module_path, function_name)
    except importlib.metadata.PackageNotFoundError:
        assert False, f"The {entry_point_name} entry point was not found."


@pytest.mark.parametrize(
    "entry_point_name",
    [
        "create_hourly_time_series",
        "create_quarterly_time_series",
        "create_charge_links",
        "create_charge_price_points",
        "create_energy_results",
        "create_monthly_amounts",
        "create_wholesale_results",
        "create_metering_point_periods",
        "create_zip",
    ],
)
def test__installed_package__can_load_entry_point(
    installed_package: None,
    entry_point_name: str,
) -> None:
    _assert_entry_point_exists(entry_point_name)


def test_get_report_id():
    report_id = "1234"
    assert get_report_id_from_args(["--report-id", report_id]) == report_id
    assert get_report_id_from_args(["--report-id", report_id, "--other", "args"]) == report_id
    assert get_report_id_from_args(["--other", "args", "--report-id", report_id]) == report_id


def test_get_report_id_with_equals():
    report_id = "1234"
    assert get_report_id_from_args(["--report-id=" + report_id]) == report_id
    assert get_report_id_from_args(["--report-id=" + report_id, "--other", "args"]) == report_id
    assert get_report_id_from_args(["--other", "args", "--report-id=" + report_id]) == report_id


def test_get_report_id_fails():
    with pytest.raises(ValueError):
        get_report_id_from_args([])
    with pytest.raises(ValueError):
        get_report_id_from_args(["--other", "args"])
