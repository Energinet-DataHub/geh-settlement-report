from enum import Enum


class ChargeTypeDataProductValue(Enum):
    """Charge type which is read from the Wholesale data product."""

    TARIFF = "tariff"
    FEE = "fee"
    SUBSCRIPTION = "subscription"
