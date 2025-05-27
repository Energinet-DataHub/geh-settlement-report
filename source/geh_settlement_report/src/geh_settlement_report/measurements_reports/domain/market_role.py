from enum import Enum


class MarketRole(Enum):
    """The market role value affects what is included in the report.

    The 'market-role' command line argument must use one of these values.
    """

    DATAHUB_ADMINISTRATOR = "datahub_administrator"
    ENERGY_SUPPLIER = "energy_supplier"
    GRID_ACCESS_PROVIDER = "grid_access_provider"
    SYSTEM_OPERATOR = "system_operator"
