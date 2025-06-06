from pyspark.sql import DataFrame, SparkSession

from geh_settlement_report.settlement_reports.infrastructure.wholesale.database_definitions import (
    WholesaleBasisDataDatabase,
    WholesaleResultsDatabase,
)


class WholesaleRepository:
    def __init__(
        self,
        spark: SparkSession,
        catalog_name: str,
    ) -> None:
        self._spark = spark
        self._catalog_name = catalog_name

    def read_metering_point_periods(self) -> DataFrame:
        return self._read_view_or_table(
            WholesaleBasisDataDatabase.DATABASE_NAME,
            WholesaleBasisDataDatabase.METERING_POINT_PERIODS_VIEW_NAME,
        )

    def read_metering_point_time_series(self) -> DataFrame:
        return self._read_view_or_table(
            WholesaleBasisDataDatabase.DATABASE_NAME,
            WholesaleBasisDataDatabase.TIME_SERIES_POINTS_VIEW_NAME,
        )

    def read_charge_price_points(self) -> DataFrame:
        return self._read_view_or_table(
            WholesaleBasisDataDatabase.DATABASE_NAME,
            WholesaleBasisDataDatabase.CHARGE_PRICE_POINTS_VIEW_NAME,
        )

    def read_charge_link_periods(self) -> DataFrame:
        return self._read_view_or_table(
            WholesaleBasisDataDatabase.DATABASE_NAME,
            WholesaleBasisDataDatabase.CHARGE_LINK_PERIODS_VIEW_NAME,
        )

    def read_charge_price_information_periods(self) -> DataFrame:
        return self._read_view_or_table(
            WholesaleBasisDataDatabase.DATABASE_NAME,
            WholesaleBasisDataDatabase.CHARGE_PRICE_INFORMATION_PERIODS_VIEW_NAME,
        )

    def read_energy(self) -> DataFrame:
        return self._read_view_or_table(
            WholesaleResultsDatabase.DATABASE_NAME,
            WholesaleResultsDatabase.ENERGY_V1_VIEW_NAME,
        )

    def read_latest_calculations(self) -> DataFrame:
        return self._read_view_or_table(
            WholesaleResultsDatabase.DATABASE_NAME,
            WholesaleResultsDatabase.LATEST_CALCULATIONS_BY_DAY_VIEW_NAME,
        )

    def read_energy_per_es(self) -> DataFrame:
        return self._read_view_or_table(
            WholesaleResultsDatabase.DATABASE_NAME,
            WholesaleResultsDatabase.ENERGY_PER_ES_V1_VIEW_NAME,
        )

    def read_amounts_per_charge(self) -> DataFrame:
        return self._read_view_or_table(
            WholesaleResultsDatabase.DATABASE_NAME,
            WholesaleResultsDatabase.AMOUNTS_PER_CHARGE_VIEW_NAME,
        )

    def read_monthly_amounts_per_charge_v1(self) -> DataFrame:
        return self._read_view_or_table(
            WholesaleResultsDatabase.DATABASE_NAME,
            WholesaleResultsDatabase.MONTHLY_AMOUNTS_PER_CHARGE_VIEW_NAME,
        )

    def read_total_monthly_amounts_v1(self) -> DataFrame:
        return self._read_view_or_table(
            WholesaleResultsDatabase.DATABASE_NAME,
            WholesaleResultsDatabase.TOTAL_MONTHLY_AMOUNTS_VIEW_NAME,
        )

    def _read_view_or_table(
        self,
        database_name: str,
        table_name: str,
    ) -> DataFrame:
        name = f"{self._catalog_name}.{database_name}.{table_name}"
        return self._spark.read.format("delta").table(name)
