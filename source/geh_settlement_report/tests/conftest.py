import os
from pathlib import Path
from typing import Generator

import geh_common.telemetry.logging_configuration
import pytest
from geh_common.telemetry.logging_configuration import (
    configure_logging,
)
from geh_common.testing.spark.spark_test_session import get_spark_test_session
from pyspark.sql import SparkSession

from tests.constants import TESTS_PATH


@pytest.fixture(scope="session")
def tests_path() -> Path:
    """
    Returns the tests folder path.
    Please note that this only works if current folder haven't been changed prior using
    `os.chdir()`. The correctness also relies on the prerequisite that this function is
    actually located in a file located directly in the tests folder.
    """
    return TESTS_PATH


@pytest.fixture(autouse=True)
def dummy_logging(request, monkeypatch: pytest.MonkeyPatch):
    """Ensure that logging hooks don't fail due to _TRACER_NAME not being set."""
    # skip if we don't want dummylogging
    if "no_dummy_logging" in request.keywords:
        yield

    env = {
        "CLOUD_ROLE_NAME": "test_role",
        "APPLICATIONINSIGHTS_CONNECTION_STRING": "connection_string",
        "SUBSYSTEM": "test_subsystem",
    }

    with pytest.MonkeyPatch.context() as mp:
        mp.setattr(os, "environ", env)
        mp.setattr(geh_common.telemetry.logging_configuration, "configure_azure_monitor", lambda *args, **kwargs: None)
        mp.setattr(geh_common.telemetry.logging_configuration, "get_is_instrumented", lambda *args, **kwargs: False)
        configure_logging(cloud_role_name="test_role", subsystem="test_subsystem")
        yield


# pytest-xdist plugin does not work with SparkSession as a fixture. The session scope is not supported.
# Therefore, we need to create a global variable to store the Spark session and data directory.
# This is a workaround to avoid creating a new Spark session for each test.
_spark, data_dir = get_spark_test_session()


@pytest.fixture(scope="session")
def spark() -> Generator[SparkSession, None, None]:
    yield _spark
    _spark.stop()
