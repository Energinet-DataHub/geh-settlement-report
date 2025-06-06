import os
from pathlib import Path
from typing import Generator

import geh_common.telemetry.logging_configuration
import pytest
from geh_common.telemetry.logging_configuration import (
    configure_logging,
)
from geh_common.testing.dataframes import AssertDataframesConfiguration
from geh_common.testing.delta_lake import create_database
from geh_common.testing.delta_lake.delta_lake_operations import create_table
from geh_common.testing.spark.spark_test_session import get_spark_test_session
from pyspark.sql import SparkSession

from tests.constants import TESTS_PATH
from tests.external_data_products import ExternalDataProducts
from tests.testsession_configuration import TestSessionConfiguration


@pytest.fixture(scope="session")
def test_session_configuration() -> TestSessionConfiguration:
    """Load the test session configuration from the testsession.local.settings.yml file.

    This is a useful feature for developers who wants to run the tests with different configurations
    on their local machine. The file is not included in the repository, so it's up to the developer to create it.
    """
    settings_file_path = TESTS_PATH / "testsession.local.settings.yml"
    return TestSessionConfiguration.load(settings_file_path)


@pytest.fixture(scope="session")
def assert_dataframes_configuration(
    test_session_configuration: TestSessionConfiguration,
) -> AssertDataframesConfiguration:
    """This fixture is used for comparing data frames in scenario tests.

    It's mainly specific to the scenario tests. The fixture is placed here to avoid code duplication."""
    return AssertDataframesConfiguration(
        show_actual_and_expected_count=test_session_configuration.scenario_tests.show_actual_and_expected_count,
        show_actual_and_expected=test_session_configuration.scenario_tests.show_actual_and_expected,
        show_columns_when_actual_and_expected_are_equal=test_session_configuration.scenario_tests.show_columns_when_actual_and_expected_are_equal,
    )


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
        return

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


@pytest.fixture(scope="session")
def external_dataproducts_created(spark: SparkSession) -> None:
    for database_name in ExternalDataProducts.get_all_database_names():
        create_database(spark, database_name)

    for dataproduct in ExternalDataProducts.get_all_data_products():
        create_table(
            spark,
            database_name=dataproduct.database_name,
            table_name=dataproduct.view_name,
            schema=dataproduct.schema,
        )


# pytest-xdist plugin does not work with SparkSession as a fixture. The session scope is not supported.
# Therefore, we need to create a global variable to store the Spark session and data directory.
# This is a workaround to avoid creating a new Spark session for each test.
_spark, data_dir = get_spark_test_session()


@pytest.fixture(scope="session")
def spark() -> Generator[SparkSession, None, None]:
    yield _spark
    _spark.stop()
