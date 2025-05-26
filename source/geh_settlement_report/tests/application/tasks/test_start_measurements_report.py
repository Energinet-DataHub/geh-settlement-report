import sys
import uuid

import pytest
from geh_common.testing.spark.mocks import MockDBUtils

from geh_settlement_report.measurements_reports.entry_point import start_measurements_report


def test_start_measurements_report(
    monkeypatch: pytest.MonkeyPatch,
    tmp_path_factory: pytest.TempPathFactory,
    dummy_logging: None,
):
    # Arrange
    report_id = uuid.uuid4().hex
    output_path = tmp_path_factory.mktemp("measurements_report_output")
    monkeypatch.setattr(
        "geh_settlement_report.measurements_reports.application.tasks.measurements_report_task.get_dbutils",
        lambda _: MockDBUtils(),
    )
    monkeypatch.setattr(
        sys,
        "argv",
        [
            "entry_point.py",
            f"--report-id={report_id}",
            "--period-start=2025-01-01",
            "--period-end=2025-01-31",
            "--grid-area-codes=123,456",
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
    assert (output_path / report_id).with_suffix(".zip").exists(), "Report CSV file was not created"
