from geh_settlement_report.settlement_reports.domain.utils.csv_column_names import CsvColumnNames
from geh_settlement_report.settlement_reports.domain.utils.market_role import MarketRole


def order_by_columns(
    requesting_actor_market_role: MarketRole,
) -> list[str]:
    order_by_column_names = [
        CsvColumnNames.grid_area_code_in_metering_points_csv,
        CsvColumnNames.metering_point_type,
        CsvColumnNames.settlement_method,
        CsvColumnNames.metering_point_from_date,
    ]
    if requesting_actor_market_role in [
        MarketRole.SYSTEM_OPERATOR,
        MarketRole.DATAHUB_ADMINISTRATOR,
    ]:
        order_by_column_names.insert(1, CsvColumnNames.energy_supplier_id)

    return order_by_column_names
