from geh_common.telemetry import Logger, use_span
from pyspark.sql import DataFrame
from pyspark.sql import functions as F

import geh_settlement_report.settlement_reports.domain.utils.map_to_csv_naming as market_naming
from geh_settlement_report.settlement_reports.domain.utils.csv_column_names import (
    CsvColumnNames,
    EphemeralColumns,
)
from geh_settlement_report.settlement_reports.domain.utils.map_from_dict import (
    map_from_dict,
)
from geh_settlement_report.settlement_reports.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)

log = Logger(__name__)


@use_span()
def prepare_for_csv(
    monthly_amounts: DataFrame,
    create_ephemeral_grid_area_column: bool,
) -> DataFrame:
    select_columns = [
        map_from_dict(market_naming.CALCULATION_TYPES_TO_ENERGY_BUSINESS_PROCESS)[
            F.col(DataProductColumnNames.calculation_type)
        ].alias(CsvColumnNames.calculation_type),
        map_from_dict(market_naming.CALCULATION_TYPES_TO_PROCESS_VARIANT)[
            F.col(DataProductColumnNames.calculation_type)
        ].alias(CsvColumnNames.correction_settlement_number),
        F.col(DataProductColumnNames.grid_area_code).alias(CsvColumnNames.grid_area_code),
        F.col(DataProductColumnNames.energy_supplier_id).alias(CsvColumnNames.energy_supplier_id),
        F.col(DataProductColumnNames.time).alias(CsvColumnNames.time),
        F.col(DataProductColumnNames.resolution).alias(CsvColumnNames.resolution),
        F.col(DataProductColumnNames.quantity_unit).alias(CsvColumnNames.quantity_unit),
        F.col(DataProductColumnNames.currency).alias(CsvColumnNames.currency),
        F.col(DataProductColumnNames.amount).alias(CsvColumnNames.amount),
        map_from_dict(market_naming.CHARGE_TYPES)[F.col(DataProductColumnNames.charge_type)].alias(
            CsvColumnNames.charge_type
        ),
        F.col(DataProductColumnNames.charge_code).alias(CsvColumnNames.charge_code),
        F.col(DataProductColumnNames.charge_owner_id).alias(CsvColumnNames.charge_owner_id),
    ]

    if create_ephemeral_grid_area_column:
        select_columns.append(
            F.col(DataProductColumnNames.grid_area_code).alias(EphemeralColumns.grid_area_code_partitioning),
        )

    return monthly_amounts.select(select_columns)
