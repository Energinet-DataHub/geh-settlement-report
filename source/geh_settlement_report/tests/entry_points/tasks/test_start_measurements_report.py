import sys
import uuid

import pytest
from geh_common.testing.spark.mocks import MockDBUtils

from geh_settlement_report.entry_points.entry_point import start_measurements_report
from geh_settlement_report.entry_points.job_args.measurements_report_args import MeasurementsReportArgs


@pytest.fixture
def mock_dbutils(monkeypatch):
    # src/geh_settlement_report/entry_points/tasks/task_factory.py
    monkeypatch.setattr("geh_settlement_report.entry_points.entry_point.get_dbutils", lambda _: MockDBUtils())
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

    return MeasurementsReportArgs()


def test_start_measurements_report(default_measurements_report_args, mock_dbutils):
    start_measurements_report()
