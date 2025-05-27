from enum import Enum


class TaskType(Enum):
    """Databricks tasks that can be executed."""

    TimeSeriesHourly = 1
    TimeSeriesQuarterly = 2
    MeteringPointPeriods = 3
    ChargeLinks = 4
    ChargePricePoints = 5
    EnergyResults = 6
    WholesaleResults = 7
    MonthlyAmounts = 8
    Zip = 9
