from dataclasses import dataclass
from datetime import datetime
from decimal import Decimal

from pyspark.sql import DataFrame, SparkSession

from geh_settlement_report.settlement_reports.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)
from geh_settlement_report.settlement_reports.infrastructure.wholesale.data_values import (
    CalculationTypeDataProductValue,
    ChargeTypeDataProductValue,
)
from geh_settlement_report.settlement_reports.infrastructure.wholesale.schemas.charge_price_points_v1 import (
    charge_price_points_v1,
)


@dataclass
class ChargePricePointsRow:
    calculation_id: str
    calculation_type: CalculationTypeDataProductValue
    calculation_version: int
    charge_key: str
    charge_code: str
    charge_type: ChargeTypeDataProductValue
    charge_owner_id: str
    charge_price: Decimal
    charge_time: datetime


def create(
    spark: SparkSession,
    rows: ChargePricePointsRow | list[ChargePricePointsRow],
) -> DataFrame:
    if not isinstance(rows, list):
        rows = [rows]

    row_list = []
    for row in rows:
        row_list.append(
            {
                DataProductColumnNames.calculation_id: row.calculation_id,
                DataProductColumnNames.calculation_type: row.calculation_type.value,
                DataProductColumnNames.calculation_version: row.calculation_version,
                DataProductColumnNames.charge_key: row.charge_key,
                DataProductColumnNames.charge_code: row.charge_code,
                DataProductColumnNames.charge_type: row.charge_type.value,
                DataProductColumnNames.charge_owner_id: row.charge_owner_id,
                DataProductColumnNames.charge_price: row.charge_price,
                DataProductColumnNames.charge_time: row.charge_time,
            }
        )

    return spark.createDataFrame(row_list, charge_price_points_v1)
