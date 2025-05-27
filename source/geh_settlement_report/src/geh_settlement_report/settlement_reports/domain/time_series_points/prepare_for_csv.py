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
    METERING_POINT_TYPES,
)
from geh_settlement_report.settlement_reports.domain.utils.market_role import MarketRole
from geh_settlement_report.settlement_reports.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)
from geh_settlement_report.settlement_reports.infrastructure.wholesale.data_values import (
    MeteringPointResolutionDataProductValue,
)

log = Logger(__name__)


@use_span()
def prepare_for_csv(
    filtered_time_series_points: DataFrame,
    metering_point_resolution: MeteringPointResolutionDataProductValue,
    time_zone: str,
    requesting_actor_market_role: MarketRole,
) -> DataFrame:
    desired_number_of_quantity_columns = _get_desired_quantity_column_count(metering_point_resolution)

    filtered_time_series_points = filtered_time_series_points.withColumn(
        CsvColumnNames.time,
        get_start_of_day(DataProductColumnNames.observation_time, time_zone),
    )

    win = Window.partitionBy(
        DataProductColumnNames.grid_area_code,
        DataProductColumnNames.energy_supplier_id,
        DataProductColumnNames.metering_point_id,
        DataProductColumnNames.metering_point_type,
        CsvColumnNames.time,
    ).orderBy(DataProductColumnNames.observation_time)
    filtered_time_series_points = filtered_time_series_points.withColumn(
        "chronological_order", F.row_number().over(win)
    )

    pivoted_df = (
        filtered_time_series_points.groupBy(
            DataProductColumnNames.grid_area_code,
            DataProductColumnNames.energy_supplier_id,
            DataProductColumnNames.metering_point_id,
            DataProductColumnNames.metering_point_type,
            CsvColumnNames.time,
        )
        .pivot(
            "chronological_order",
            list(range(1, desired_number_of_quantity_columns + 1)),
        )
        .agg(F.first(DataProductColumnNames.quantity))
    )

    quantity_column_names = [
        F.col(str(i)).alias(f"{CsvColumnNames.energy_quantity}{i}")
        for i in range(1, desired_number_of_quantity_columns + 1)
    ]

    csv_df = pivoted_df.select(
        F.col(DataProductColumnNames.grid_area_code).alias(EphemeralColumns.grid_area_code_partitioning),
        F.col(DataProductColumnNames.energy_supplier_id).alias(CsvColumnNames.energy_supplier_id),
        F.col(DataProductColumnNames.metering_point_id).alias(CsvColumnNames.metering_point_id),
        map_from_dict(METERING_POINT_TYPES)[F.col(DataProductColumnNames.metering_point_type)].alias(
            CsvColumnNames.metering_point_type
        ),
        F.col(CsvColumnNames.time),
        *quantity_column_names,
    )

    if requesting_actor_market_role in [
        MarketRole.GRID_ACCESS_PROVIDER,
        MarketRole.ENERGY_SUPPLIER,
    ]:
        csv_df = csv_df.drop(CsvColumnNames.energy_supplier_id)

    return csv_df


def _get_desired_quantity_column_count(
    resolution: MeteringPointResolutionDataProductValue,
) -> int:
    if resolution == MeteringPointResolutionDataProductValue.HOUR:
        return 25
    elif resolution == MeteringPointResolutionDataProductValue.QUARTER:
        return 25 * 4
    else:
        raise ValueError(f"Unknown time series resolution: {resolution}")
