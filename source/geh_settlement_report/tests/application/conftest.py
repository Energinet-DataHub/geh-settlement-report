import os
import shutil
import subprocess

import pytest

from tests.constants import PROJECT_PATH


@pytest.fixture(scope="session")
def installed_package():
    """Ensures that the settlement report package is installed (after building it)."""
    # Create and activate the virtual environment
    venv_name = ".settlement-report-pytest"
    subprocess.call(["uv", "venv", venv_name])
    subprocess.call(f"source {venv_name}/bin/activate", shell=True, executable="/bin/bash")

    # Build the package wheel
    os.chdir(PROJECT_PATH)
    subprocess.call("uv build", shell=True, executable="/bin/bash")

    # Uninstall the package in case it was left by a cancelled test suite
    subprocess.call(
        f"uv pip uninstall --python={venv_name}/bin/python geh_settlement_report",
        shell=True,
        executable="/bin/bash",
    )

    # Install wheel, which will also create console scripts for invoking
    # the entry points of the package
    subprocess.call(
        f"uv pip install --python={venv_name}/bin/python {PROJECT_PATH}/dist/geh_settlement_report*.whl",
        shell=True,
        executable="/bin/bash",
    )

    yield None

    # Deactivate virtual environment upon test suite tear down
    subprocess.call("deactivate", shell=True, executable="/bin/bash")
    subprocess.call(["rm", "-rf", ".settlement-report-pytest"])
    shutil.rmtree(".settlement-report-pytest", ignore_errors=True)
    shutil.rmtree(PROJECT_PATH / "dist", ignore_errors=True)
