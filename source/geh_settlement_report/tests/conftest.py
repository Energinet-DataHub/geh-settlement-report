import json
import logging
import os
import shutil
import sys
import uuid
from pathlib import Path
from typing import Callable, Generator
from unittest import mock

import pytest
import yaml
from geh_common.testing.spark.spark_test_session import get_spark_test_session
from pyspark.sql import SparkSession

from geh_settlement_report.settlement_reports.application.job_args.calculation_type import CalculationType
from geh_settlement_report.settlement_reports.application.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from geh_settlement_report.settlement_reports.domain.utils.market_role import MarketRole
from tests.constants import PROJECT_PATH
from tests.data_seeding import (
    standard_balance_fixing_scenario_data_generator,
    standard_wholesale_fixing_scenario_data_generator,
)
from tests.data_seeding.write_test_data import (
    write_amounts_per_charge_to_delta_table,
    write_charge_link_periods_to_delta_table,
    write_charge_price_information_periods_to_delta_table,
    write_charge_price_points_to_delta_table,
    write_energy_per_es_to_delta_table,
    write_energy_to_delta_table,
    write_latest_calculations_by_day_to_delta_table,
    write_metering_point_periods_to_delta_table,
    write_metering_point_time_series_to_delta_table,
    write_monthly_amounts_per_charge_to_delta_table,
    write_total_monthly_amounts_to_delta_table,
)
from tests.dbutils_fixture import DBUtilsFixture
from tests.integration_test_configuration import IntegrationTestConfiguration


@pytest.fixture(scope="session")
def dbutils() -> DBUtilsFixture:
    """
    Returns a DBUtilsFixture instance that can be used to mock dbutils.
    """
    return DBUtilsFixture()


@pytest.fixture(scope="session", autouse=True)
def cleanup_before_tests(
    input_database_location: str,
):
    if os.path.exists(input_database_location):
        shutil.rmtree(input_database_location)

    yield

    # Add cleanup code to be run after the tests


@pytest.fixture(scope="function")
def standard_wholesale_fixing_scenario_args(
    settlement_reports_output_path: str,
    monkeypatch: pytest.MonkeyPatch,
) -> SettlementReportArgs:
    grid_area_uuid = {
        standard_wholesale_fixing_scenario_data_generator.GRID_AREAS[0]: uuid.UUID(
            standard_wholesale_fixing_scenario_data_generator.CALCULATION_ID
        ).hex,
        standard_wholesale_fixing_scenario_data_generator.GRID_AREAS[1]: uuid.UUID(
            standard_wholesale_fixing_scenario_data_generator.CALCULATION_ID
        ).hex,
    }

    args = [
        f"--report-id={str(uuid.uuid4())}",
        f"--period-start={standard_wholesale_fixing_scenario_data_generator.FROM_DATE}",
        f"--period-end={standard_wholesale_fixing_scenario_data_generator.TO_DATE}",
        f"--calculation-type={CalculationType.WHOLESALE_FIXING.value}",
        f"--calculation-id-by-grid-area={json.dumps(grid_area_uuid)}",
        "--split-report-by-grid-area",
        f"--requesting-actor-market-role={MarketRole.SYSTEM_OPERATOR.value}",  # using system operator since it is more complex (requires filter based on charge owner)
        f"--requesting-actor-id={standard_wholesale_fixing_scenario_data_generator.CHARGE_OWNER_ID_WITHOUT_TAX}",
        f"--settlement-reports-output-path={settlement_reports_output_path}",
        "--include-basis-data",
    ]
    monkeypatch.setenv("TIME_ZONE", "Europe/Copenhagen")
    monkeypatch.setenv("CATALOG_NAME", "spark_catalog")

    monkeypatch.setattr(sys, "argv", ["program"] + args)

    return SettlementReportArgs()


@pytest.fixture(scope="function")
def standard_wholesale_fixing_scenario_datahub_admin_args(
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
) -> SettlementReportArgs:
    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = MarketRole.DATAHUB_ADMINISTRATOR
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = None
    return standard_wholesale_fixing_scenario_args


@pytest.fixture(scope="function")
def standard_wholesale_fixing_scenario_energy_supplier_args(
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
) -> SettlementReportArgs:
    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = MarketRole.ENERGY_SUPPLIER
    energy_supplier_id = standard_wholesale_fixing_scenario_data_generator.ENERGY_SUPPLIER_IDS[0]
    standard_wholesale_fixing_scenario_args.requesting_actor_id = energy_supplier_id
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = [energy_supplier_id]
    return standard_wholesale_fixing_scenario_args


@pytest.fixture(scope="function")
def standard_wholesale_fixing_scenario_grid_access_provider_args(
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
) -> SettlementReportArgs:
    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = MarketRole.GRID_ACCESS_PROVIDER
    standard_wholesale_fixing_scenario_args.requesting_actor_id = (
        standard_wholesale_fixing_scenario_data_generator.CHARGE_OWNER_ID_WITH_TAX
    )
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = None
    return standard_wholesale_fixing_scenario_args


@pytest.fixture(scope="function")
def standard_wholesale_fixing_scenario_system_operator_args(
    standard_wholesale_fixing_scenario_args: SettlementReportArgs,
) -> SettlementReportArgs:
    standard_wholesale_fixing_scenario_args.requesting_actor_market_role = MarketRole.SYSTEM_OPERATOR
    standard_wholesale_fixing_scenario_args.requesting_actor_id = (
        standard_wholesale_fixing_scenario_data_generator.CHARGE_OWNER_ID_WITHOUT_TAX
    )
    standard_wholesale_fixing_scenario_args.energy_supplier_ids = None
    return standard_wholesale_fixing_scenario_args


@pytest.fixture(scope="function")
def standard_balance_fixing_scenario_args(
    settlement_reports_output_path: str, monkeypatch: pytest.MonkeyPatch
) -> SettlementReportArgs:
    # grid_areas = [
    #     int(standard_wholesale_fixing_scenario_data_generator.GRID_AREAS[0]),
    #     int(standard_wholesale_fixing_scenario_data_generator.GRID_AREAS[1]),
    # ]

    grid_areas = ",".join(standard_wholesale_fixing_scenario_data_generator.GRID_AREAS)

    args = [
        f"--report-id={str(uuid.uuid4())}",
        f"--period-start={standard_balance_fixing_scenario_data_generator.FROM_DATE}",
        f"--period-end={standard_balance_fixing_scenario_data_generator.TO_DATE}",
        f"--calculation-type={CalculationType.BALANCE_FIXING.value}",
        f"--grid-area-codes={grid_areas}",
        "--split-report-by-grid-area",
        f"--requesting-actor-market-role={MarketRole.SYSTEM_OPERATOR.value}",
        "--requesting-actor-id=1212121212121",
        f"--settlement-reports-output-path={settlement_reports_output_path}",
        "--include-basis-data",
    ]
    monkeypatch.setenv("TIME_ZONE", "Europe/Copenhagen")
    monkeypatch.setenv("CATALOG_NAME", "spark_catalog")

    monkeypatch.setattr(sys, "argv", ["program"] + args)

    return SettlementReportArgs()


@pytest.fixture(scope="function")
def standard_balance_fixing_scenario_grid_access_provider_args(
    standard_balance_fixing_scenario_args: SettlementReportArgs,
) -> SettlementReportArgs:
    standard_balance_fixing_scenario_args.requesting_actor_market_role = MarketRole.GRID_ACCESS_PROVIDER
    standard_balance_fixing_scenario_args.requesting_actor_id = (
        standard_wholesale_fixing_scenario_data_generator.CHARGE_OWNER_ID_WITH_TAX
    )
    standard_balance_fixing_scenario_args.energy_supplier_ids = None
    return standard_balance_fixing_scenario_args


@pytest.fixture(scope="session")
def standard_balance_fixing_scenario_data_written_to_delta(
    spark: SparkSession,
    input_database_location: str,
) -> None:
    time_series_points_df = standard_balance_fixing_scenario_data_generator.create_metering_point_time_series(spark)
    write_metering_point_time_series_to_delta_table(spark, time_series_points_df, input_database_location)

    metering_point_periods = standard_balance_fixing_scenario_data_generator.create_metering_point_periods(spark)
    write_metering_point_periods_to_delta_table(spark, metering_point_periods, input_database_location)

    energy_df = standard_balance_fixing_scenario_data_generator.create_energy(spark)
    write_energy_to_delta_table(spark, energy_df, input_database_location)

    energy_per_es_df = standard_balance_fixing_scenario_data_generator.create_energy_per_es(spark)
    write_energy_per_es_to_delta_table(spark, energy_per_es_df, input_database_location)

    latest_calculations_by_day = standard_balance_fixing_scenario_data_generator.create_latest_calculations(spark)
    write_latest_calculations_by_day_to_delta_table(spark, latest_calculations_by_day, input_database_location)


@pytest.fixture(scope="session")
def standard_wholesale_fixing_scenario_data_written_to_delta(
    spark: SparkSession,
    input_database_location: str,
) -> None:
    metering_point_periods = standard_wholesale_fixing_scenario_data_generator.create_metering_point_periods(spark)
    write_metering_point_periods_to_delta_table(spark, metering_point_periods, input_database_location)

    time_series_points = standard_wholesale_fixing_scenario_data_generator.create_metering_point_time_series(spark)
    write_metering_point_time_series_to_delta_table(spark, time_series_points, input_database_location)

    charge_link_periods = standard_wholesale_fixing_scenario_data_generator.create_charge_link_periods(spark)
    write_charge_link_periods_to_delta_table(spark, charge_link_periods, input_database_location)

    charge_price_points = standard_wholesale_fixing_scenario_data_generator.create_charge_price_points(spark)
    write_charge_price_points_to_delta_table(spark, charge_price_points, input_database_location)

    charge_price_information_periods = (
        standard_wholesale_fixing_scenario_data_generator.create_charge_price_information_periods(spark)
    )
    write_charge_price_information_periods_to_delta_table(
        spark, charge_price_information_periods, input_database_location
    )

    energy = standard_wholesale_fixing_scenario_data_generator.create_energy(spark)
    write_energy_to_delta_table(spark, energy, input_database_location)

    energy_per_es = standard_wholesale_fixing_scenario_data_generator.create_energy_per_es(spark)
    write_energy_per_es_to_delta_table(spark, energy_per_es, input_database_location)

    amounts_per_charge = standard_wholesale_fixing_scenario_data_generator.create_amounts_per_charge(spark)
    write_amounts_per_charge_to_delta_table(spark, amounts_per_charge, input_database_location)

    monthly_amounts_per_charge_df = standard_wholesale_fixing_scenario_data_generator.create_monthly_amounts_per_charge(
        spark
    )
    write_monthly_amounts_per_charge_to_delta_table(spark, monthly_amounts_per_charge_df, input_database_location)
    total_monthly_amounts_df = standard_wholesale_fixing_scenario_data_generator.create_total_monthly_amounts(spark)
    write_total_monthly_amounts_to_delta_table(spark, total_monthly_amounts_df, input_database_location)


@pytest.fixture(scope="session")
def file_path_finder() -> Callable[[str], str]:
    """
    Returns the path of the file.
    Please note that this only works if current folder haven't been changed prior using
    `os.chdir()`. The correctness also relies on the prerequisite that this function is
    actually located in a file located directly in the tests folder.
    """

    def finder(file: str) -> str:
        return os.path.dirname(os.path.normpath(file))

    return finder


@pytest.fixture(scope="session")
def source_path(file_path_finder: Callable[[str], str]) -> str:
    """
    Returns the <repo-root>/source folder path.
    Please note that this only works if current folder haven't been changed prior using
    `os.chdir()`. The correctness also relies on the prerequisite that this function is
    actually located in a file located directly in the tests folder.
    """
    return file_path_finder(f"{__file__}/../..")


@pytest.fixture(scope="session")
def settlement_report_path(source_path: str) -> Path:
    """
    Returns the source/databricks/ folder path.
    Please note that this only works if current folder haven't been changed prior using
    `os.chdir()`. The correctness also relies on the prerequisite that this function is
    actually located in a file located directly in the tests folder.
    """
    return PROJECT_PATH


@pytest.fixture(scope="session")
def contracts_path(settlement_report_path: str) -> str:
    """
    Returns the source/contract folder path.
    Please note that this only works if current folder haven't been changed prior using
    `os.chdir()`. The correctness also relies on the prerequisite that this function is
    actually located in a file located directly in the tests folder.
    """
    return f"{settlement_report_path}/contracts"


@pytest.fixture(scope="session")
def test_files_folder_path(tests_path: str) -> str:
    return f"{tests_path}/test_files"


@pytest.fixture(scope="session")
def settlement_reports_output_path(data_lake_path: str) -> str:
    return f"{data_lake_path}/settlement_reports_output"


@pytest.fixture(scope="session")
def input_database_location(data_lake_path: str) -> str:
    return f"{data_lake_path}/input_database"


@pytest.fixture(scope="session")
def data_lake_path(tests_path: str, worker_id: str) -> str:
    return f"{tests_path}/__data_lake__/{worker_id}"


@pytest.fixture(scope="session")
def tests_path(settlement_report_path: str) -> str:
    """
    Returns the tests folder path.
    Please note that this only works if current folder haven't been changed prior using
    `os.chdir()`. The correctness also relies on the prerequisite that this function is
    actually located in a file located directly in the tests folder.
    """
    return f"{settlement_report_path}/tests"


@pytest.fixture(scope="session")
def settlement_report_job_container_path(source_path: str) -> str:
    """
    Returns the <repo-root>/source folder path.
    Please note that this only works if current folder haven't been changed prior using
    `os.chdir()`. The correctness also relies on the prerequisite that this function is
    actually located in a file located directly in the tests folder.
    """
    return PROJECT_PATH


# pytest-xdist plugin does not work with SparkSession as a fixture. The session scope is not supported.
# Therefore, we need to create a global variable to store the Spark session and data directory.
# This is a workaround to avoid creating a new Spark session for each test.
_spark, data_dir = get_spark_test_session()


@pytest.fixture(scope="session")
def spark() -> Generator[SparkSession, None, None]:
    yield _spark
    _spark.stop()


@pytest.fixture(scope="session")
def env_args_fixture() -> dict[str, str]:
    env_args = {
        "CLOUD_ROLE_NAME": "test_role",
        "APPLICATIONINSIGHTS_CONNECTION_STRING": "connection_string",
        "SUBSYSTEM": "test_subsystem",
    }
    return env_args


@pytest.fixture(scope="session")
def script_args_fixture() -> list[str]:
    sys_argv = [
        "program_name",
        "--force_configuration",
        "false",
        "--orchestration-instance-id",
        "4a540892-2c0a-46a9-9257-c4e13051d76a",
    ]
    return sys_argv


@pytest.fixture(scope="function")
def script_args_fixture_integration_test() -> list[str]:
    sys_argv = [
        "program_name",
        "--force_configuration",
        "false",
        "--orchestration-instance-id",
        str(uuid.uuid4()),
        "--report-id",
        str(uuid.uuid4()),
        "--period-start",
        "2024-01-01T23:00:00Z",
        "--period-end",
        "2024-01-02T23:00:00Z",
        "--calculation-type",
        "wholesale_fixing",
        "--requesting-actor-market-role",
        "system_operator",
        "--requesting-actor-id",
        "5790001330552",
        "--calculation-id-by-grid-area",
        f"804={uuid.uuid4()}",
        "--split-report-by-grid-area",
        "true",
        "--prevent-large-text-files",
        "false",
        "--time-zone",
        "Europe/Copenhagen",
        "--catalog-name",
        "spark_catalog",
        "--settlement-reports-output-path",
        "/workspaces/geh-settlement-report/source/settlement_report_python/tests/__data_lake__/master/settlement_reports_output",
        "--include-basis-data",
        "true",
    ]
    return sys_argv


@pytest.fixture(autouse=True)
def configure_dummy_logging(env_args_fixture, script_args_fixture) -> None:
    """Ensure that logging hooks don't fail due to _TRACER_NAME not being set."""

    from geh_common.telemetry.logging_configuration import (
        configure_logging,
    )

    with (
        mock.patch("sys.argv", script_args_fixture),
        mock.patch.dict("os.environ", env_args_fixture, clear=False),
        mock.patch(
            "geh_common.telemetry.logging_configuration.configure_azure_monitor"
        ),  # Patching call to configure_azure_monitor in order to not actually connect to app. insights.
    ):
        configure_logging(
            cloud_role_name=env_args_fixture["CLOUD_ROLE_NAME"],
            subsystem=env_args_fixture["SUBSYSTEM"],
        )


@pytest.fixture(scope="function")
def clean_up_logging():
    """
    Function that cleans up the Logging module prior to running integration tests, so logging can be reconfigured after use of configure_dummy_logging fixture
    """
    from geh_common.telemetry.logging_configuration import (
        set_extras,
        set_is_instrumented,
        set_tracer,
        set_tracer_name,
    )

    set_extras({})
    set_is_instrumented(False)
    set_tracer(None)
    set_tracer_name("")


@pytest.fixture(scope="session")
def integration_test_configuration(tests_path: str) -> IntegrationTestConfiguration:
    """
    Load settings for integration tests either from a local YAML settings file or from environment variables.
    Proceeds even if certain Azure-related keys are not present in the settings file.
    """

    settings_file_path = Path(tests_path) / "integrationtest.local.settings.yml"

    def load_settings_from_env() -> dict:
        return {
            key: os.getenv(key)
            for key in [
                "AZURE_KEYVAULT_URL",
                "AZURE_CLIENT_ID",
                "AZURE_CLIENT_SECRET",
                "AZURE_TENANT_ID",
                "AZURE_SUBSCRIPTION_ID",
            ]
            if os.getenv(key) is not None
        }

    settings = _load_settings_from_file(settings_file_path) or load_settings_from_env()

    # Set environment variables from loaded settings
    for key, value in settings.items():
        if value is not None:
            os.environ[key] = value

    if "AZURE_KEYVAULT_URL" in settings:
        return IntegrationTestConfiguration(azure_keyvault_url=settings["AZURE_KEYVAULT_URL"])

    logging.error(
        f"Integration test configuration could not be loaded from {settings_file_path} or environment variables."
    )
    raise Exception(
        "Failed to load integration test settings. Ensure that the Azure Key Vault URL is provided in the settings file or as an environment variable."
    )


def _load_settings_from_file(file_path: Path) -> dict:
    if file_path.exists():
        with file_path.open() as stream:
            return yaml.safe_load(stream)
    else:
        return {}
