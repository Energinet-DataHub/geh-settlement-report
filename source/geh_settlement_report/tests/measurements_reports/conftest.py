import sys
from pathlib import Path

import pytest
import yaml
from geh_common.data_products.electricity_market_reports_input import measurements_report_metering_point_periods_v1
from geh_common.data_products.measurements_core.measurements_gold import current_v1
from geh_common.testing.dataframes import read_csv
from geh_common.testing.scenario_testing import TestCase, TestCases
from pyspark.sql import SparkSession

from geh_settlement_report.measurements_reports.application.job_args.measurements_report_args import (
    MeasurementsReportArgs,
)
from geh_settlement_report.measurements_reports.domain.measurements_reports.calculation import execute


@pytest.fixture(scope="module")
def test_cases(spark: SparkSession, request: pytest.FixtureRequest, dummy_logging, tmp_path_factory) -> TestCases:
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
        monkeypatch.setenv("CATALOG_NAME", "spark_catalog")
        monkeypatch.setenv("OUTPUT_PATH", str(output_path))

        # Execute the logic
        args = MeasurementsReportArgs()
        result = execute(
            args,
            calculated_measurements=measurements_gold_current_v1,
            metering_point_periods=measurements_report_metering_point_periods,
        )

        # Return test cases
        return TestCases(
            [
                TestCase(
                    expected_csv_path=f"{scenario_path}/then/measurements_report_800_8000000000000_01-05-2025_02-05-2025.csv",
                    actual=result,
                ),
            ]
        )
