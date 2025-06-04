from datetime import datetime, timezone
from decimal import Decimal

from pyspark.sql import SparkSession


def seed(spark: SparkSession) -> None:
    _seed_electricity_market(spark)
    _seed_calculated_measurements(spark)


def _seed_electricity_market(spark: SparkSession) -> None:
    spark.sql(f"""
                INSERT INTO spark_catalog.electricity_market_reports_input.measurements_report_metering_point_periods_v1
                VALUES
                (
                    '800',
                    '170000000000000201',
                    'consumption',
                    'P15M',
                    '1000000000000',
                    'connected',
                    'kWh',
                    NULL,
                    NULL,
                    '{datetime(2016, 1, 1, 23, tzinfo=timezone.utc).strftime("%Y-%m-%d %H:%M:%S")}',
                    NULL
                ),
                (
                    '801',
                    '170000000000000202',
                    'consumption',
                    'P15M',
                    '1000000000000',
                    'connected',
                    'kWh',
                    NULL,
                    NULL,
                    '{datetime(2016, 1, 1, 23, tzinfo=timezone.utc).strftime("%Y-%m-%d %H:%M:%S")}',
                    NULL
                )
            """)


def _seed_calculated_measurements(spark: SparkSession) -> None:
    spark.sql(f"""
                INSERT INTO spark_catalog.measurements_gold.current_v1
                VALUES (
                    '170000000000000201',
                    '{datetime(2025, 5, 1, 17, 15, 00, tzinfo=timezone.utc).strftime("%Y-%m-%d %H:%M:%S")}',
                    '{Decimal(2.125)}',
                    'measured',
                    'consumption'
                ),
                (
                    '170000000000000201',
                    '{datetime(2025, 5, 1, 17, 30, 00, tzinfo=timezone.utc).strftime("%Y-%m-%d %H:%M:%S")}',
                    '{Decimal(2.125)}',
                    'measured',
                    'consumption'
                )
              """)
