from geh_common.data_products.electricity_market_reports_input import measurements_report_metering_point_periods_v1
from geh_common.testing.dataframes import assert_contract
from pyspark.sql import SparkSession

from geh_settlement_report.common.domain.model.measurements_report_metering_point_periods import (
    MeasurementsReportMeteringPointPeriods,
)


class ElectricityMarketRepository:
    def __init__(
        self,
        spark: SparkSession,
        catalog_name: str,
    ) -> None:
        self._spark = spark
        self._catalog_name = catalog_name

    def read_measurements_report_metering_point_periods(self) -> MeasurementsReportMeteringPointPeriods:
        table_name = f"{self._catalog_name}.{measurements_report_metering_point_periods_v1.database_name}.{measurements_report_metering_point_periods_v1.view_name}"
        df = self._spark.read.format("delta").table(table_name)
        assert_contract(df.schema, measurements_report_metering_point_periods_v1.schema)
        return MeasurementsReportMeteringPointPeriods(df)
