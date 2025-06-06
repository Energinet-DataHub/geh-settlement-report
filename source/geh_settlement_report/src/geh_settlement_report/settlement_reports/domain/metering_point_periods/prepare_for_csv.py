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
    METERING_POINT_TYPES,
    SETTLEMENT_METHODS,
)
from geh_settlement_report.settlement_reports.domain.utils.market_role import MarketRole
from geh_settlement_report.settlement_reports.infrastructure.wholesale.column_names import (
    DataProductColumnNames,
)

log = Logger(__name__)


@use_span()
def prepare_for_csv(
    metering_point_periods: DataFrame,
    requesting_actor_market_role: MarketRole,
) -> DataFrame:
    columns = [
        F.col(DataProductColumnNames.grid_area_code).alias(EphemeralColumns.grid_area_code_partitioning),
        F.col(DataProductColumnNames.metering_point_id).alias(CsvColumnNames.metering_point_id),
        F.col(DataProductColumnNames.from_date).alias(CsvColumnNames.metering_point_from_date),
        F.col(DataProductColumnNames.to_date).alias(CsvColumnNames.metering_point_to_date),
        F.col(DataProductColumnNames.grid_area_code).alias(CsvColumnNames.grid_area_code_in_metering_points_csv),
        map_from_dict(METERING_POINT_TYPES)[F.col(DataProductColumnNames.metering_point_type)].alias(
            CsvColumnNames.metering_point_type
        ),
        map_from_dict(SETTLEMENT_METHODS)[F.col(DataProductColumnNames.settlement_method)].alias(
            CsvColumnNames.settlement_method
        ),
    ]
    if requesting_actor_market_role is MarketRole.GRID_ACCESS_PROVIDER:
        columns.insert(
            5,
            F.col(DataProductColumnNames.to_grid_area_code).alias(CsvColumnNames.to_grid_area_code),
        )
        columns.insert(
            6,
            F.col(DataProductColumnNames.from_grid_area_code).alias(CsvColumnNames.from_grid_area_code),
        )

    if requesting_actor_market_role in [
        MarketRole.SYSTEM_OPERATOR,
        MarketRole.DATAHUB_ADMINISTRATOR,
    ]:
        columns.append(F.col(DataProductColumnNames.energy_supplier_id).alias(CsvColumnNames.energy_supplier_id))

    csv_df = metering_point_periods.select(columns)

    return csv_df
