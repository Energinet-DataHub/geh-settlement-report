from geh_settlement_report.domain.utils.csv_column_names import CsvColumnNames
from geh_settlement_report.domain.utils.market_role import MarketRole


def order_by_columns(requesting_actor_market_role: MarketRole) -> list[str]:
    order_by_column_names = [
        CsvColumnNames.metering_point_type,
        CsvColumnNames.metering_point_id,
        CsvColumnNames.time,
    ]
    if requesting_actor_market_role in [
        MarketRole.SYSTEM_OPERATOR,
        MarketRole.DATAHUB_ADMINISTRATOR,
    ]:
        order_by_column_names.insert(0, CsvColumnNames.energy_supplier_id)

    return order_by_column_names
