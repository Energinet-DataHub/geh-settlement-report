import sys
from pathlib import Path
from zipfile import ZipFile
from zoneinfo import ZoneInfo

import pytest
import yaml
from geh_common.data_products.electricity_market_reports_input import measurements_report_metering_point_periods_v1
from geh_common.data_products.measurements_core.measurements_gold import current_v1
from geh_common.testing.dataframes import read_csv
from geh_common.testing.scenario_testing import TestCase, TestCases
from geh_common.testing.spark.mocks import MockDBUtils
from pyspark.sql import SparkSession

from geh_settlement_report.measurements_reports.application.job_args.measurements_report_args import (
    MeasurementsReportArgs,
)
from geh_settlement_report.measurements_reports.domain.calculation import execute
from geh_settlement_report.measurements_reports.domain.file_name_factory import file_name_factory


@pytest.fixture(scope="module")
def test_cases(spark: SparkSession, request: pytest.FixtureRequest, tmp_path_factory) -> TestCases:
    """Fixture used for scenario tests. Learn more in package `testcommon.etl`."""

    # Get the path to the scenario
    scenario_path = str(Path(request.module.__file__).parent)
    output_path = tmp_path_factory.mktemp("measurements_reports")

    # Read input data
    measurements_gold_current_v1 = read_csv(
        spark,
        f"{scenario_path}/when/measurements_gold/current_v1.csv",
        current_v1.schema,
    )
    measurements_report_metering_point_periods = read_csv(
        spark,
        f"{scenario_path}/when/electricity_market_reports_input/measurements_report_metering_point_periods_v1.csv",
        measurements_report_metering_point_periods_v1.schema,
    )

    # Set scenario arguments
    with open(f"{scenario_path}/when/scenario_parameters.yml") as f:
        scenario_parameters = yaml.safe_load(f)

    with pytest.MonkeyPatch.context() as monkeypatch:
        sysargs = [x for k, v in scenario_parameters.items() for x in [f"--{k.replace('_', '-')}", str(v)]]
        monkeypatch.setattr(sys, "argv", ["program"] + sysargs)
        monkeypatch.setattr(
            "geh_settlement_report.measurements_reports.domain.calculation.get_dbutils",
            lambda _: MockDBUtils(),
        )
        monkeypatch.setenv("CATALOG_NAME", "spark_catalog")
        monkeypatch.setenv("OUTPUT_PATH", str(output_path))

        # Execute the logic
        args = MeasurementsReportArgs()
        result = execute(
            spark,
            args,
            calculated_measurements=measurements_gold_current_v1,
            metering_point_periods=measurements_report_metering_point_periods,
        )
        result.show(1000, truncate=False)

        zip_path = Path(args.output_path) / f"{args.report_id}.zip"

        assert zip_path.exists(), f"Zip file {zip_path} was not created."
        assert zip_path.is_file(), f"Expected {zip_path} to be a file."
        assert zip_path.stat().st_size > 0, f"Zip file {zip_path} is empty."

        with ZipFile(zip_path, "r") as zip_file:
            # Check if the expected CSV file is in the zip
            files = [Path(f).name for f in zip_file.namelist()]
            assert len(files) == 1, f"Expected exactly one file in zip, found {len(files)} files: {files}"

            expected_csv_name = f"{file_name_factory(args)}.csv"
            assert expected_csv_name in files, f"Expected CSV file {expected_csv_name} not found in zip."

        # Create expected CSV file name
        timezone_obj = ZoneInfo(args.time_zone)
        period_start_local_time = scenario_parameters["period_start"].astimezone(timezone_obj).strftime("%d-%m-%Y")
        period_end_local_time = scenario_parameters["period_end"].astimezone(timezone_obj).strftime("%d-%m-%Y")
        expected_csv_name = f"measurements_report_{scenario_parameters['requesting_actor_id']}_{period_start_local_time}_{period_end_local_time}.csv"

        # Return test cases
        return TestCases(
            [
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/{expected_csv_name}",
                    actual=result,
                ),
            ]
        )
