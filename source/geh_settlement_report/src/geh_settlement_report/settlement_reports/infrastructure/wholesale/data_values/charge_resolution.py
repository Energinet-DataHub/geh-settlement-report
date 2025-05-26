from enum import Enum


class ChargeResolutionDataProductValue(Enum):
    """Time resolution of the charges, which is read from the Wholesale data product."""

    MONTH = "P1M"
    DAY = "P1D"
    HOUR = "PT1H"
