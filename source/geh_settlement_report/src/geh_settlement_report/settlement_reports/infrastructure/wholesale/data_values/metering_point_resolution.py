from enum import Enum


class MeteringPointResolutionDataProductValue(Enum):
    """Resolution values as defined for metering points in the data product(s)."""

    HOUR = "PT1H"
    QUARTER = "PT15M"
