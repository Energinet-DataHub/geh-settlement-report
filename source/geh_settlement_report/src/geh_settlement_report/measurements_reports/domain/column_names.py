class MeasurementsReportColumnNames:
    """Column names for the MeasurementsReport table."""

    grid_area_code = "GRID AREA ID"
    metering_point_id = "METERING POINT ID"
    metering_point_type = "METERING POINT TYPE"
    resolution = "RESOLUTION"
    energy_supplier_id = "ENERGY SUPPLIER ID"
    physical_status = "PHYSICAL STATUS"
    observation_time = "DATE TIME STAMP"
    quantity = "QUANTITY"
    quantity_quality = "QUALITY"
    unit = "UNIT"


class MeteringPointPeriodsColumnNames:
    """Column names for the MeteringPointPeriods table."""

    grid_area_code = "grid_area_code"
    metering_point_id = "metering_point_id"
    metering_point_type = "metering_point_type"
    resolution = "resolution"
    energy_supplier_id = "energy_supplier_id"
    physical_status = "physical_status"
    quantity_unit = "quantity_unit"
    from_grid_area_code = "from_grid_area_code"
    to_grid_area_code = "to_grid_area_code"
    period_from_date = "period_from_date"
    period_to_date = "period_to_date"


class MeasurementsGoldCurrentV1ColumnNames:
    """Column names for the MeasurementsGoldCurrentV1 data product."""

    metering_point_id = "metering_point_id"
    observation_time = "observation_time"
    quantity = "quantity"
    quality = "quality"
    metering_point_type = "metering_point_type"
