from geh_settlement_report.settlement_reports.domain.utils.csv_column_names import CsvColumnNames
from geh_settlement_report.settlement_reports.domain.utils.market_role import MarketRole


def order_by_columns(
    requesting_actor_market_role: MarketRole,
) -> list[str]:
    order_by_column_names = [
        CsvColumnNames.metering_point_type,
        CsvColumnNames.metering_point_id,
        CsvColumnNames.charge_owner_id,
        CsvColumnNames.charge_code,
        CsvColumnNames.charge_link_from_date,
    ]
    if requesting_actor_market_role in [
        MarketRole.SYSTEM_OPERATOR,
        MarketRole.DATAHUB_ADMINISTRATOR,
    ]:
        order_by_column_names.insert(0, CsvColumnNames.energy_supplier_id)

    return order_by_column_names
