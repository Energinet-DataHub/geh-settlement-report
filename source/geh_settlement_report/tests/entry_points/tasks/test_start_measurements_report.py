import shutil
from pathlib import Path

import pytest

from geh_settlement_report.entry_points.entry_point import start_measurements_report


def _remove_path_prefix(path: str) -> str:
    """Remove the prefix from the path if it exists."""
    path, *rest = str(path).split(":", 1)
    if len(rest) > 0:
        return Path("".join(rest))
    else:
        return Path(path)


class MockDBUtils:
    @property
    def fs(self):
        class MockFS:
            def ls(self, path):
                return [f for f in Path(path).iterdir()]

            def mv(self, src: str | Path, dst: str | Path):
                src = _remove_path_prefix(src)
                dst = _remove_path_prefix(dst)
                shutil.move(Path(src), Path(dst))

            def cp(self, src, dst):
                if Path(src).is_dir():
                    shutil.copytree(src, dst)
                else:
                    shutil.copy(src, dst)

        return MockFS()


@pytest.fixture
def mock_dbutils(monkeypatch):
    # src/geh_settlement_report/entry_points/tasks/task_factory.py
    monkeypatch.setattr("geh_settlement_report.entry_points.entry_point.get_dbutils", lambda _: MockDBUtils())
    return MockDBUtils()


def test_start_measurements_report(mock_dbutils):
    start_measurements_report()
