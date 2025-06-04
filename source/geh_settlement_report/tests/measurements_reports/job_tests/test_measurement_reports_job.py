import shutil
import sys
import uuid
import zipfile

import pytest
from geh_common.testing.spark.mocks import MockDBUtils
from pyspark.sql import SparkSession

from geh_settlement_report.measurements_reports.entry_point import start_measurements_report
from tests.measurements_reports.job_tests.seeding import seed_data


def test_start_measurements_report(
    spark: SparkSession,
    monkeypatch: pytest.MonkeyPatch,
    tmp_path_factory: pytest.TempPathFactory,
    external_dataproducts_created: None,  # Used implicitly
) -> None:
    # Arrange
    report_id = uuid.uuid4().hex
    output_path = tmp_path_factory.mktemp("measurements_report_output")
    result_file = output_path / f"{report_id}.zip"
    monkeypatch.setattr(
        "geh_settlement_report.measurements_reports.domain.calculation.get_dbutils",
        lambda _: MockDBUtils(),
    )

    monkeypatch.setattr(
        "geh_settlement_report.measurements_reports.application.tasks.measurements_report_task.initialize_spark",
        lambda: spark,
    )
    monkeypatch.setattr(
        sys,
        "argv",
        [
            "entry_point.py",
            f"--report-id={report_id}",
            "--period-start=2016-01-01",
            "--period-end=2026-01-01",
            "--grid-area-codes=[800]",
            "--requesting-actor-id=1234567890123",
            "--energy-supplier-ids=[1000000000000]",
        ],
    )
    monkeypatch.setenv("OUTPUT_PATH", str(output_path))
    monkeypatch.setenv("CATALOG_NAME", "spark_catalog")
    monkeypatch.setenv("TIME_ZONE", "Europe/Copenhagen")
    monkeypatch.setenv(
        "APPLICATIONINSIGHTS_CONNECTION_STRING", "InstrumentationKey=12345678-1234-1234-1234-123456789012"
    )

    # Seed electricity market and gold tables
    seed_data(spark)

    # Act
    start_measurements_report()

    zip_file = zipfile.ZipFile(result_file, "r")
    zip_file.extractall(output_path / f"{report_id}_extraction")

    # Assert
    df = spark.read.csv(f"{output_path}/{report_id}_extraction/{zip_file.namelist()[0]}", header=True)
    assert df.count() > 0, "The csv file from the zip contains no rows"

    # Clean up
    shutil.rmtree(output_path, ignore_errors=True)
