from datetime import datetime, timezone
from decimal import Decimal

from geh_common.data_products.electricity_market_reports_input import measurements_report_metering_point_periods_v1
from geh_common.data_products.measurements_core.measurements_gold import current_v1
from pyspark.sql import SparkSession

from tests import SPARK_CATALOG_NAME


def seed_data(spark: SparkSession) -> None:
    _seed_electricity_market(spark)
    _seed_calculated_measurements(spark)


def _seed_electricity_market(spark: SparkSession) -> None:
    spark.sql(f"""
                INSERT INTO {SPARK_CATALOG_NAME}.{measurements_report_metering_point_periods_v1.database_name}.{measurements_report_metering_point_periods_v1.view_name} 
                VALUES
                (
                    '800',
                    '000000000000000017',
                    'consumption',
                    'PT1H',
                    '1000000000000',
                    'connected',
                    'kVArh',
                    NULL,
                    NULL,
                    '{datetime(2016, 1, 1, 23, tzinfo=timezone.utc).strftime("%Y-%m-%d %H:%M:%S")}',
                    NULL
                )
            """)


def _seed_calculated_measurements(spark: SparkSession) -> None:
    spark.sql(f"""
                INSERT INTO {SPARK_CATALOG_NAME}.{current_v1.database_name}.{current_v1.view_name}
                VALUES (
                    '000000000000000017',
                    '{datetime(2016, 1, 1, 23, tzinfo=timezone.utc).strftime("%Y-%m-%d %H:%M:%S")}',
                    '{Decimal(100)}',
                    'estimated',
                    'consumption'
                )
              """)
