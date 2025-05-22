from geh_common.telemetry import Logger, use_span
from pyspark.sql import DataFrame
from pyspark.sql import functions as F

from geh_settlement_report.settlement_reports.domain.utils.csv_column_names import (
    CsvColumnNames,
    EphemeralColumns,
)
from geh_settlement_report.settlement_reports.domain.utils.map_from_dict import (
    map_from_dict,
)
from geh_settlement_report.settlement_reports.domain.utils.map_to_csv_naming import (
    CHARGE_TYPES,
    METERING_POINT_TYPES,
)
from geh_settlement_report.settlement_reports.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)

log = Logger(__name__)


@use_span()
def prepare_for_csv(
    charge_link_periods: DataFrame,
) -> DataFrame:
    columns = [
        F.col(DataProductColumnNames.grid_area_code).alias(EphemeralColumns.grid_area_code_partitioning),
        F.col(DataProductColumnNames.metering_point_id).alias(CsvColumnNames.metering_point_id),
        map_from_dict(METERING_POINT_TYPES)[F.col(DataProductColumnNames.metering_point_type)].alias(
            CsvColumnNames.metering_point_type
        ),
        map_from_dict(CHARGE_TYPES)[F.col(DataProductColumnNames.charge_type)].alias(CsvColumnNames.charge_type),
        F.col(DataProductColumnNames.charge_owner_id).alias(CsvColumnNames.charge_owner_id),
        F.col(DataProductColumnNames.charge_code).alias(CsvColumnNames.charge_code),
        F.col(DataProductColumnNames.quantity).alias(CsvColumnNames.charge_quantity),
        F.col(DataProductColumnNames.from_date).alias(CsvColumnNames.charge_link_from_date),
        F.col(DataProductColumnNames.to_date).alias(CsvColumnNames.charge_link_to_date),
    ]
    if DataProductColumnNames.energy_supplier_id in charge_link_periods.columns:
        columns.append(F.col(DataProductColumnNames.energy_supplier_id).alias(CsvColumnNames.energy_supplier_id))

    return charge_link_periods.select(columns)
