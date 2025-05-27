from geh_common.telemetry import Logger, use_span
from pyspark.sql import DataFrame, Window
from pyspark.sql import functions as F

from geh_settlement_report.settlement_reports.domain.utils.csv_column_names import (
    CsvColumnNames,
    EphemeralColumns,
)
from geh_settlement_report.settlement_reports.domain.utils.get_start_of_day import get_start_of_day
from geh_settlement_report.settlement_reports.domain.utils.map_from_dict import (
    map_from_dict,
)
from geh_settlement_report.settlement_reports.domain.utils.map_to_csv_naming import (
    CHARGE_TYPES,
    TAX_INDICATORS,
)
from geh_settlement_report.settlement_reports.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)

log = Logger(__name__)


@use_span()
def prepare_for_csv(
    filtered_charge_price_points: DataFrame,
    time_zone: str,
) -> DataFrame:
    filtered_charge_price_points = filtered_charge_price_points.withColumn(
        CsvColumnNames.time,
        get_start_of_day(DataProductColumnNames.charge_time, time_zone),
    )

    win = Window.partitionBy(
        DataProductColumnNames.grid_area_code,
        DataProductColumnNames.charge_type,
        DataProductColumnNames.charge_owner_id,
        DataProductColumnNames.charge_code,
        DataProductColumnNames.resolution,
        DataProductColumnNames.is_tax,
        CsvColumnNames.time,
    ).orderBy(DataProductColumnNames.charge_time)
    filtered_charge_price_points = filtered_charge_price_points.withColumn(
        "chronological_order", F.row_number().over(win)
    )

    pivoted_df = (
        filtered_charge_price_points.groupBy(
            DataProductColumnNames.grid_area_code,
            DataProductColumnNames.charge_type,
            DataProductColumnNames.charge_owner_id,
            DataProductColumnNames.charge_code,
            DataProductColumnNames.resolution,
            DataProductColumnNames.is_tax,
            CsvColumnNames.time,
        )
        .pivot(
            "chronological_order",
            list(range(1, 25 + 1)),
        )
        .agg(F.first(DataProductColumnNames.charge_price))
    )

    charge_price_column_names = [F.col(str(i)).alias(f"{CsvColumnNames.energy_price}{i}") for i in range(1, 25 + 1)]

    csv_df = pivoted_df.select(
        F.col(DataProductColumnNames.grid_area_code).alias(EphemeralColumns.grid_area_code_partitioning),
        map_from_dict(CHARGE_TYPES)[F.col(DataProductColumnNames.charge_type)].alias(CsvColumnNames.charge_type),
        F.col(DataProductColumnNames.charge_owner_id).alias(CsvColumnNames.charge_owner_id),
        F.col(DataProductColumnNames.charge_code).alias(CsvColumnNames.charge_code),
        F.col(DataProductColumnNames.resolution).alias(CsvColumnNames.resolution),
        map_from_dict(TAX_INDICATORS)[F.col(DataProductColumnNames.is_tax)].alias(CsvColumnNames.is_tax),
        F.col(CsvColumnNames.time),
        *charge_price_column_names,
    )

    return csv_df
