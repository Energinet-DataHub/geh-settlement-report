import pytest
from pyspark.sql import SparkSession

from geh_settlement_report.settlement_reports.application.utils.get_dbutils import get_dbutils


def test_get_dbutils__when_run_locally__raise_exception(spark: SparkSession):
    # Act
    with pytest.raises(Exception):
        get_dbutils(spark)
