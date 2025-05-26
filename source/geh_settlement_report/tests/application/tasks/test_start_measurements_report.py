import sys
import uuid

import pytest
from geh_common.testing.spark.mocks import MockDBUtils

from geh_settlement_report.measurements_reports.application.job_args.measurements_report_args import (
    MeasurementsReportArgs,
)
from geh_settlement_report.measurements_reports.entry_point import start_measurements_report


@pytest.fixture
def mock_dbutils(monkeypatch):
    monkeypatch.setattr(
        "geh_settlement_report.measurements_reports.application.tasks.measurements_report_task.get_dbutils",
        lambda _: MockDBUtils(),
    )
    return MockDBUtils()


@pytest.fixture
def default_measurements_report_args(monkeypatch: pytest.MonkeyPatch):
    monkeypatch.setattr(
        sys,
        "argv",
        [
            "entry_point.py",
            f"--report-id={uuid.uuid4().hex}",
            "--period-start=2025-01-01",
            "--period-end=2025-01-31",
            "--grid-area-codes=123,456",
        ],
    )
    monkeypatch.setenv("OUTPUT_PATH", "/tmp")
    monkeypatch.setenv("CATALOG_NAME", "spark_catalog")
    monkeypatch.setenv("TIME_ZONE", "Europe/Copenhagen")
    monkeypatch.setenv(
        "APPLICATIONINSIGHTS_CONNECTION_STRING", "InstrumentationKey=12345678-1234-1234-1234-123456789012"
    )

    return MeasurementsReportArgs()


def test_start_measurements_report(
    default_measurements_report_args,
    mock_dbutils,
    dummy_logging: None,
):
    start_measurements_report()
