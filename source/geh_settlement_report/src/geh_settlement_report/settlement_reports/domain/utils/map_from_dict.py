import itertools

from pyspark.sql import Column
from pyspark.sql import functions as F


def map_from_dict(d: dict) -> Column:
    """Convert a dictionary to a Spark map column.

    Args:
        d (dict): Dictionary to convert to a Spark map column

    Returns:
        Column: Spark map column
    """
    return F.create_map([F.lit(x) for x in itertools.chain(*d.items())])
