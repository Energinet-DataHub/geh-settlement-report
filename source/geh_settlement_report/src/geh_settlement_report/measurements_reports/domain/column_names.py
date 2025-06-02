class MeasurementsReport:
    """Column names for the MeasurementsReport table."""

    grid_area_code = "grid_area_code"
    metering_point_id = "metering_point_id"
    metering_point_type = "metering_point_type"
    resolution = "resolution"
    energy_supplier_id = "energy_supplier_id"
    physical_status = "physical_status"
    observation_time = "observation_time"
    quantity = "quantity"
    quantity_quality = "quantity_quality"
    unit = "unit"


class MeteringPointPeriods:
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


class MeasurementsGoldCurrentV1:
    """Column names for the MeasurementsGoldCurrentV1 table."""

    metering_point_id = "metering_point_id"
    observation_time = "observation_time"
    quantity = "quantity"
    quality = "quality"
    metering_point_type = "metering_point_type"
