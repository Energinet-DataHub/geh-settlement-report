# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

import importlib.metadata
from typing import Any

import pytest

from geh_settlement_report.entry_points import entry_point as module
from geh_settlement_report.entry_points.entry_point import (
    get_report_id_from_args,
)

# IMPORTANT:
# If we add/remove tests here, we also update the "retry logic" in '.docker/entrypoint.sh',
# which depends on the number of "entry point tests".


def assert_entry_point_exists(entry_point_name: str) -> Any:
    # Load the entry point function from the installed wheel
    try:
        entry_points = importlib.metadata.entry_points(group="console_scripts", name=entry_point_name)

        if not entry_points:
            assert False, f"The {entry_point_name} entry point was not found."

        # Filter for the specific entry point group and name
        matching_entry_points = [ep for ep in entry_points if ep.name == entry_point_name]

        if not matching_entry_points:
            assert False, f"The {entry_point_name} entry point was not found."

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
    assert_entry_point_exists(entry_point_name)


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
