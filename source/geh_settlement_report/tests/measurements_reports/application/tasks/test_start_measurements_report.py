import shutil
import sys
import uuid

import pytest
from geh_common.testing.spark.mocks import MockDBUtils
from pyspark.sql import SparkSession

from geh_settlement_report.measurements_reports.entry_point import start_measurements_report


def test_start_measurements_report(
    spark: SparkSession,
    monkeypatch: pytest.MonkeyPatch,
    tmp_path_factory: pytest.TempPathFactory,
):
    # Arrange
    report_id = uuid.uuid4().hex
    output_path = tmp_path_factory.mktemp("measurements_report_output")
    result_file = output_path / f"{report_id}.zip"
    monkeypatch.setattr(
        "geh_settlement_report.measurements_reports.application.tasks.measurements_report_task.get_dbutils",
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
            "--period-start=2025-01-01",
            "--period-end=2025-01-31",
            "--grid-area-codes=[123,456]",
            "--requesting-actor-id=1234567890123",
            "--energy-supplier-ids=[1234567890123]",
        ],
    )
    monkeypatch.setenv("OUTPUT_PATH", str(output_path))
    monkeypatch.setenv("CATALOG_NAME", "spark_catalog")
    monkeypatch.setenv("TIME_ZONE", "Europe/Copenhagen")
    monkeypatch.setenv(
        "APPLICATIONINSIGHTS_CONNECTION_STRING", "InstrumentationKey=12345678-1234-1234-1234-123456789012"
    )

    # Act
    start_measurements_report()

    # Assert
    assert result_file.exists(), "Report CSV file was not created"
    assert result_file.is_file(), "Report output is not a file"
    assert result_file.stat().st_size > 0, "Report CSV file is empty"

    # Clean up
    shutil.rmtree(output_path, ignore_errors=True)
