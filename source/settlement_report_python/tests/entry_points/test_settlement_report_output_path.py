from datetime import datetime
import json
import os
import sys

import pydantic
import pytest
from settlement_report_job.domain.utils.market_role import MarketRole
from settlement_report_job.entry_points.job_args.calculation_type import CalculationType
from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from dataclasses import dataclass
from pathlib import Path

from tests import PROJECT_PATH


@dataclass
class Contract:
    required: list
    optional: list


def _load_contract(path: Path):
    lines = path.read_text().splitlines()
    required_params = []
    optional_params = []
    mode = None
    for line in lines:
        if "required parameters" in line.lower():
            mode = "required"
            continue
        if "optional parameters" in line.lower():
            mode = "optional"
            continue
        if mode == "required" and line.startswith("--"):
            required_params.append(line)
        if mode == "optional" and line.startswith("--"):
            optional_params.append(line)
    return Contract(required_params, optional_params)


def _load_contracts():
    contract_path = PROJECT_PATH / "contracts"
    contracts = {}
    for p in contract_path.iterdir():
        contracts[p.stem] = _load_contract(p)
    return contracts


def _args_to_dict(args):
    args_dict = {}
    for arg in args:
        if not arg.startswith("--"):
            continue
        if "=" in arg:
            key, value = arg.split("=")
        else:
            key = arg
            value = True
        args_dict[key[2:]] = value
    return args_dict


def _assert_args(args, args_dict, env_args):
    assert str(args.report_id) == args_dict["report-id"]
    assert args.period_start == datetime.fromisoformat(args_dict["period-start"])
    assert args.period_end == datetime.fromisoformat(args_dict["period-end"])
    assert args.requesting_actor_market_role == MarketRole.ENERGY_SUPPLIER
    assert args.requesting_actor_id == args_dict["requesting-actor-id"]
    assert args.catalog_name == env_args["CATALOG_NAME"]
    assert (
        args.settlement_reports_output_path
        == env_args["SETTLEMENT_REPORTS_OUTPUT_PATH"]
    )

    # Differentiate between balance fixing and other calculation types
    if args.calculation_type == CalculationType.BALANCE_FIXING:
        assert args.grid_area_codes == [
            str(i) for i in json.loads(args_dict["grid-area-codes"])
        ]
    else:
        assert {
            k: str(v) for k, v in args.calculation_id_by_grid_area.items()
        } == json.loads(args_dict["calculation-id-by-grid-area"])

    # Optional arguments
    if args.energy_supplier_ids:
        assert args.energy_supplier_ids == [
            str(v) for v in json.loads(args_dict.get("energy-supplier-ids"))
        ]
    assert args.split_report_by_grid_area is args_dict.get(
        "split-report-by-grid-area", False
    )
    assert args.prevent_large_text_files is args_dict.get(
        "prevent-large-text-files", False
    )
    assert args.time_zone == args_dict.get("time-zone", "Europe/Copenhagen")
    assert args.include_basis_data is args_dict.get("include-basis-data", False)


@pytest.fixture
def environment_variables():
    return {
        "CATALOG_NAME": "catalog",
        "SETTLEMENT_REPORTS_OUTPUT_PATH": "/Volumes/catalog/wholesale_settlement_report_output/settlement_reports",
    }


@pytest.mark.parametrize(["_", "contract"], _load_contracts().items())
def test_settlement_report_required_args(
    _, contract: Contract, environment_variables: dict, monkeypatch: pytest.MonkeyPatch
):
    args_dict = _args_to_dict(contract.required)
    monkeypatch.setattr(
        sys,
        "argv",
        ["settlement_report_job"] + contract.required,
    )
    monkeypatch.setattr(os, "environ", environment_variables)
    args = SettlementReportArgs()
    _assert_args(args, args_dict, environment_variables)


@pytest.mark.parametrize(["_", "contract"], _load_contracts().items())
def test_settlement_report_optional_args(
    _, contract: Contract, environment_variables: dict, monkeypatch: pytest.MonkeyPatch
):
    args_dict = _args_to_dict(contract.required + contract.optional)
    monkeypatch.setattr(
        sys,
        "argv",
        ["settlement_report_job"] + contract.required + contract.optional,
    )
    monkeypatch.setattr(os, "environ", environment_variables)
    args = SettlementReportArgs()
    _assert_args(args, args_dict, environment_variables)


@pytest.mark.parametrize(["_", "contract"], _load_contracts().items())
def test_settlement_report_args_missing_env(
    _, contract: Contract, environment_variables: dict, monkeypatch: pytest.MonkeyPatch
):
    environment_variables.pop("CATALOG_NAME")
    monkeypatch.setattr(
        sys,
        "argv",
        ["settlement_report_job"] + contract.required + contract.optional,
    )
    monkeypatch.setattr(os, "environ", environment_variables)
    with pytest.raises(pydantic.ValidationError):
        SettlementReportArgs()


@pytest.mark.parametrize(["_", "contract"], _load_contracts().items())
def test_settlement_report_args_required_args_missing(
    _, contract: Contract, environment_variables: dict, monkeypatch: pytest.MonkeyPatch
):
    required_args = contract.required.copy()
    required_args.pop(0)
    monkeypatch.setattr(
        sys,
        "argv",
        ["settlement_report_job"] + required_args,
    )
    monkeypatch.setattr(os, "environ", environment_variables)
    with pytest.raises(pydantic.ValidationError):
        SettlementReportArgs()


@pytest.mark.parametrize(
    ["reason", "args"],
    [
        (
            "short_grid_area_code",
            {
                "report-id": "12345678-9fc8-409a-a169-fbd49479d718",
                "period-start": "2022-05-31T22:00:00Z",
                "period-end": "2022-06-01T22:00:00Z",
                "calculation-type": "balance_fixing",
                "requesting-actor-market-role": "energy_supplier",
                "requesting-actor-id": "1234567890123",
                "grid-area-codes": "[0, 805]",
            },
        ),
        (
            "long_grid_area_code",
            {
                "report-id": "12345678-9fc8-409a-a169-fbd49479d718",
                "period-start": "2022-05-31T22:00:00Z",
                "period-end": "2022-06-01T22:00:00Z",
                "calculation-type": "balance_fixing",
                "requesting-actor-market-role": "energy_supplier",
                "requesting-actor-id": "1234567890123",
                "grid-area-codes": "[012341, 805]",
            },
        ),
    ],
)
def test_grid_area_code_validation(
    reason, args, environment_variables: dict, monkeypatch: pytest.MonkeyPatch
):
    sys_args = [f"--{k}={v}" for k, v in args.items()]
    monkeypatch.setattr(sys, "argv", ["settlement_report_job"] + sys_args)
    monkeypatch.setattr(os, "environ", environment_variables)
    with pytest.raises(pydantic.ValidationError) as e:
        SettlementReportArgs()
        assert "Unknown grid area code:" in str(e.value)
