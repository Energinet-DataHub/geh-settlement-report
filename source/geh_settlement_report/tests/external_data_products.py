from geh_common.data_products.electricity_market_reports_input import measurements_report_metering_point_periods_v1
from geh_common.data_products.measurements_core.measurements_gold import current_v1 as current

from tests import DataProduct


class ExternalDataProducts:
    CURRENT_MEASUREMENTS: DataProduct = DataProduct(
        database_name=current.database_name,
        view_name=current.view_name,
        schema=current.schema,
    )
    MEASUREMENTS_REPORT_METERING_POINT_PERIODS_V1: DataProduct = DataProduct(
        database_name=measurements_report_metering_point_periods_v1.database_name,
        view_name=measurements_report_metering_point_periods_v1.view_name,
        schema=measurements_report_metering_point_periods_v1.schema,
    )

    @staticmethod
    def get_all_database_names() -> list[str]:
        return list(
            set(
                getattr(getattr(ExternalDataProducts, attr), "database_name")
                for attr in dir(ExternalDataProducts)
                if isinstance(getattr(ExternalDataProducts, attr), DataProduct)
            )
        )

    @staticmethod
    def get_all_data_products() -> list[DataProduct]:
        return [
            getattr(ExternalDataProducts, attr)
            for attr in dir(ExternalDataProducts)
            if isinstance(getattr(ExternalDataProducts, attr), DataProduct)
        ]
