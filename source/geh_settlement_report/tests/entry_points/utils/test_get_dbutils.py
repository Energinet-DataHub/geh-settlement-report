import pytest
from geh_settlement_report.entry_points.utils.get_dbutils import get_dbutils
from pyspark.sql import SparkSession


def test_get_dbutils__when_run_locally__raise_exception(spark: SparkSession):
    # Act
    with pytest.raises(Exception):
        get_dbutils(spark)
