from enum import Enum


class CalculationType(Enum):
    """The job parameter values must correspond with these values."""

    BALANCE_FIXING = "balance_fixing"
    WHOLESALE_FIXING = "wholesale_fixing"
    FIRST_CORRECTION_SETTLEMENT = "first_correction_settlement"
    SECOND_CORRECTION_SETTLEMENT = "second_correction_settlement"
    THIRD_CORRECTION_SETTLEMENT = "third_correction_settlement"
