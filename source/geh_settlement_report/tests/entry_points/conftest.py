import os
import subprocess
from typing import Generator

import pytest


@pytest.fixture(scope="session")
def virtual_environment() -> Generator:
    """Fixture ensuring execution in a virtual environment.
    Uses `virtualenv` instead of conda environments due to problems
    activating the virtual environment from pytest."""

    # Create and activate the virtual environment
    subprocess.call(["uv", "venv", ".settlement-report-pytest"])
    subprocess.call(
        "source .settlement-report-pytest/bin/activate",
        shell=True,
        executable="/bin/bash",
    )

    yield None

    # Deactivate virtual environment upon test suite tear down
    subprocess.call("deactivate", shell=True, executable="/bin/bash")
    subprocess.call(["rm", "-rf", ".settlement-report-pytest"])


@pytest.fixture(scope="session")
def installed_package(
    virtual_environment: Generator, settlement_report_job_container_path: str
) -> None:
    """Ensures that the settlement report package is installed (after building it)."""

    # Build the package wheel
    os.chdir(settlement_report_job_container_path)
    subprocess.call("uv build", shell=True, executable="/bin/bash")

    # Uninstall the package in case it was left by a cancelled test suite
    subprocess.call(
        "uv pip uninstall geh_settlement_report",
        shell=True,
        executable="/bin/bash",
    )

    # Install wheel, which will also create console scripts for invoking
    # the entry points of the package
    subprocess.call(
        f"uv pip install {settlement_report_job_container_path}/dist/geh_settlement_report*.whl",
        shell=True,
        executable="/bin/bash",
    )
