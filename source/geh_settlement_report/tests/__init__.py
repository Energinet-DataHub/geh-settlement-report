from dataclasses import dataclass
from pathlib import Path
from typing import Any

PROJECT_PATH = Path(__file__).parent.parent

SPARK_CATALOG_NAME = "spark_catalog"


@dataclass
class DataProduct:
    database_name: str
    view_name: str
    schema: Any
