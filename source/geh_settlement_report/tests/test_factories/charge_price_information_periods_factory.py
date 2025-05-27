from dataclasses import dataclass
from datetime import datetime

from pyspark.sql import DataFrame, SparkSession

from geh_settlement_report.settlement_reports.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)
from geh_settlement_report.settlement_reports.infrastructure.wholesale.data_values import (
    CalculationTypeDataProductValue,
    ChargeResolutionDataProductValue,
    ChargeTypeDataProductValue,
)
from geh_settlement_report.settlement_reports.infrastructure.wholesale.schemas import (
    charge_price_information_periods_v1,
)


@dataclass
class ChargePriceInformationPeriodsRow:
    calculation_id: str
    calculation_type: CalculationTypeDataProductValue
    calculation_version: int
    charge_key: str
    charge_code: str
    charge_type: ChargeTypeDataProductValue
    charge_owner_id: str
    resolution: ChargeResolutionDataProductValue
    is_tax: bool
    from_date: datetime
    to_date: datetime


def create(
    spark: SparkSession,
    rows: ChargePriceInformationPeriodsRow | list[ChargePriceInformationPeriodsRow],
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
                DataProductColumnNames.resolution: row.resolution.value,
                DataProductColumnNames.is_tax: row.is_tax,
                DataProductColumnNames.from_date: row.from_date,
                DataProductColumnNames.to_date: row.to_date,
            }
        )

    return spark.createDataFrame(row_list, charge_price_information_periods_v1)
