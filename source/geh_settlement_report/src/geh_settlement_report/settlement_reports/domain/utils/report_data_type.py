from enum import Enum


class ReportDataType(Enum):
    """Types of data that can be included in a settlement report.

    Used to distinguish between different types of data in the report.
    """

    TimeSeriesHourly = 1
    TimeSeriesQuarterly = 2
    MeteringPointPeriods = 3
    ChargeLinks = 4
    EnergyResults = 5
    WholesaleResults = 6
    MonthlyAmounts = 7
    ChargePricePoints = 8
