from dataclasses import dataclass
from datetime import datetime

from geh_settlement_report.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)
from geh_settlement_report.infrastructure.wholesale.data_values import (
    CalculationTypeDataProductValue,
    ChargeTypeDataProductValue,
)
from geh_settlement_report.infrastructure.wholesale.schemas import (
    charge_link_periods_v1,
)
from pyspark.sql import DataFrame, SparkSession


@dataclass
class ChargeLinkPeriodsRow:
    calculation_id: str
    calculation_type: CalculationTypeDataProductValue
    calculation_version: int
    charge_key: str
    charge_code: str
    charge_type: ChargeTypeDataProductValue
    charge_owner_id: str
    metering_point_id: str
    quantity: int
    from_date: datetime
    to_date: datetime


def create(
    spark: SparkSession,
    rows: ChargeLinkPeriodsRow | list[ChargeLinkPeriodsRow],
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
                DataProductColumnNames.metering_point_id: row.metering_point_id,
                DataProductColumnNames.quantity: row.quantity,
                DataProductColumnNames.from_date: row.from_date,
                DataProductColumnNames.to_date: row.to_date,
            }
        )

    return spark.createDataFrame(row_list, charge_link_periods_v1)
