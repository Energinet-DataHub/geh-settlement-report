import pytest
from geh_common.testing.spark.mocks import MockDBUtils

from geh_settlement_report.entry_points.entry_point import start_measurements_report


@pytest.fixture
def mock_dbutils(monkeypatch):
    # src/geh_settlement_report/entry_points/tasks/task_factory.py
    monkeypatch.setattr("geh_settlement_report.entry_points.entry_point.get_dbutils", lambda _: MockDBUtils())
    return MockDBUtils()


def test_start_measurements_report(mock_dbutils):
    start_measurements_report()
