from dataclasses import dataclass
from datetime import datetime

from pyspark.sql import DataFrame, SparkSession

from geh_settlement_report.settlement_reports.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)
from geh_settlement_report.settlement_reports.infrastructure.wholesale.data_values import (
    CalculationTypeDataProductValue,
)
from geh_settlement_report.settlement_reports.infrastructure.wholesale.schemas.latest_calculations_by_day_v1 import (
    latest_calculations_by_day_v1,
)


@dataclass
class LatestCalculationsPerDayRow:
    calculation_id: str
    calculation_type: CalculationTypeDataProductValue
    calculation_version: int
    grid_area_code: str
    start_of_day: datetime


def create(
    spark: SparkSession,
    rows: LatestCalculationsPerDayRow | list[LatestCalculationsPerDayRow],
) -> DataFrame:
    if not isinstance(rows, list):
        rows = [rows]

    data_rows = []
    for row in rows:
        data_rows.append(
            {
                DataProductColumnNames.calculation_id: row.calculation_id,
                DataProductColumnNames.calculation_type: row.calculation_type.value,
                DataProductColumnNames.calculation_version: row.calculation_version,
                DataProductColumnNames.grid_area_code: row.grid_area_code,
                DataProductColumnNames.start_of_day: row.start_of_day,
            }
        )

    return spark.createDataFrame(data_rows, latest_calculations_by_day_v1)
