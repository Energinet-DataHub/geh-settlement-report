from dataclasses import dataclass
from datetime import datetime
from decimal import Decimal
from typing import Any, Iterable

from pyspark.sql import DataFrame, SparkSession

from geh_settlement_report.settlement_reports.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)
from geh_settlement_report.settlement_reports.infrastructure.wholesale.data_values import (
    CalculationTypeDataProductValue,
)
from geh_settlement_report.settlement_reports.infrastructure.wholesale.schemas.total_monthly_amounts_v1 import (
    total_monthly_amounts_v1,
)


@dataclass
class TotalMonthlyAmountsRow:
    """
    Data specification for creating wholesale test data.
    """

    calculation_id: str
    calculation_type: CalculationTypeDataProductValue
    calculation_version: int
    result_id: str
    grid_area_code: str
    energy_supplier_id: str
    charge_owner_id: str
    currency: str
    time: datetime
    amount: Decimal


def create(spark: SparkSession, data_spec: TotalMonthlyAmountsRow) -> DataFrame:
    row: Iterable[Any] = [
        {
            DataProductColumnNames.calculation_id: data_spec.calculation_id,
            DataProductColumnNames.calculation_type: data_spec.calculation_type.value,
            DataProductColumnNames.calculation_version: data_spec.calculation_version,
            DataProductColumnNames.result_id: data_spec.result_id,
            DataProductColumnNames.grid_area_code: data_spec.grid_area_code,
            DataProductColumnNames.energy_supplier_id: data_spec.energy_supplier_id,
            DataProductColumnNames.charge_owner_id: data_spec.charge_owner_id,
            DataProductColumnNames.currency: data_spec.currency,
            DataProductColumnNames.time: data_spec.time,
            DataProductColumnNames.amount: data_spec.amount,
        }
    ]

    return spark.createDataFrame(row, schema=total_monthly_amounts_v1)
