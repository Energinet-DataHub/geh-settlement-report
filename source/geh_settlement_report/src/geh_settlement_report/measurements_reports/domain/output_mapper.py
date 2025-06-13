from itertools import chain

from geh_common.pyspark.transformations import convert_from_utc
from pyspark.sql import DataFrame
from pyspark.sql import functions as F
from pyspark.sql.window import Window

from geh_settlement_report.measurements_reports.domain.calculation import MeasurementsReportArgs
from geh_settlement_report.measurements_reports.domain.column_names import MeasurementsReportColumnNames

# Define all mappings at module level
MAPPINGS = {
    MeasurementsReportColumnNames.metering_point_type: {
        "ve_production": "D01",
        "analysis": "D02",
        "net_production": "D05",
        "supply_to_grid": "D06",
        "consumption_from_grid": "D07",
        "wholesale_services_or_information": "D08",
        "own_production": "D09",
        "net_from_grid": "D10",
        "net_to_grid": "D11",
        "total_consumption": "D12",
        "electrical_heating": "D14",
        "net_consumption": "D15",
        "other_consumption": "D17",
        "other_production": "D18",
        "capacity_settlement": "D19",
        "exchange_reactive_energy": "D20",
        "collective_net_production": "D21",
        "collective_net_consumption": "D22",
        "activated_downregulation": "D23",
        "activated_upregulation": "D24",
        "actual_consumption": "D25",
        "actual_production": "D26",
        "consumption": "E17",
        "production": "E18",
        "exchange": "E20",
    },
    MeasurementsReportColumnNames.unit: {
        "kWh": "kWh",
        "kVArh": "kVArh",
        "TNE": "Tonne",
    },
    MeasurementsReportColumnNames.physical_status: {"connected": "E22", "disconnected": "E23"},
    MeasurementsReportColumnNames.quantity_quality: {
        "measured": "Measured",
        "missing": "Missing",
        "estimated": "Estimated",
        "calculated": "Calculated",
    },
}


def map_to_output(
    args: MeasurementsReportArgs,
    result: DataFrame,
) -> DataFrame:
    # Create mapping expressions for each column
    # This transforms our Python dictionaries into Spark SQL map literals that can be used in DataFrame operations
    # Each key-value pair from the mapping dictionaries becomes a Spark map for lookups
    mapping_exprs = {
        col: F.create_map([F.lit(x) for x in chain(*[(k, v) for k, v in mapping_data.items()])])
        for col, mapping_data in MAPPINGS.items()
    }

    # Apply the mappings to translate internal values to standardized output codes
    # For each column that needs mapping, apply the corresponding lookup expression
    # This replaces values like "consumption" with standard codes like "E17"
    for col, expr in mapping_exprs.items():
        result = result.withColumn(col, expr.getItem(F.col(col)))

    # Map quality values to their standardized codes
    # TODO: Uncomment until scenario tests are updated to handle quality mapping
    # result = quality_mapping(result)

    # Convert observation timestamps from UTC to the specified timezone
    # Then format the timestamp into a standardized date-time string (dd-MM-yyyy HH:mm)
    result = convert_from_utc(result, args.time_zone)
    result = result.withColumn(
        MeasurementsReportColumnNames.observation_time,
        F.date_format(F.col(MeasurementsReportColumnNames.observation_time), "dd-MM-yyyy HH:mm"),
    )

    # Handle null quantity values by replacing them with 0.000
    result = result.withColumn(
        MeasurementsReportColumnNames.quantity, F.coalesce(F.col(MeasurementsReportColumnNames.quantity), F.lit(0.000))
    )

    # Sort the output data for consistent reporting presentation
    result = result.orderBy(
        MeasurementsReportColumnNames.grid_area_code,
        MeasurementsReportColumnNames.metering_point_type,
        MeasurementsReportColumnNames.metering_point_id,
        MeasurementsReportColumnNames.observation_time,
    )

    return result


def quality_mapping(df: DataFrame) -> DataFrame:
    # Group by the specified columns to find rows with the same grid_area, metering_point_id and type
    window_spec = Window.partitionBy(
        MeasurementsReportColumnNames.grid_area_code,
        MeasurementsReportColumnNames.metering_point_id,
        MeasurementsReportColumnNames.metering_point_type,
    )

    # Check if any row in each group has "missing" quality
    has_missing = F.max(
        F.when(F.col(MeasurementsReportColumnNames.quantity_quality) == "Missing", 1).otherwise(0)
    ).over(window_spec)

    # Check if any row in each group has "estimated" quality
    has_estimated = F.max(
        F.when(F.col(MeasurementsReportColumnNames.quantity_quality) == "Estimated", 1).otherwise(0)
    ).over(window_spec)

    # Apply mapping logic:
    # 1. If any row has "missing" quality, all rows in that group become "Missing"
    # 2. If any row has "estimated" quality (and no "missing"), all rows become "Estimated"
    # 3. Otherwise, keep the original quality
    df = df.withColumn(
        MeasurementsReportColumnNames.quantity_quality,
        F.when(has_missing > 0, "Missing")
        .when(has_estimated > 0, "Estimated")
        .otherwise(F.col(MeasurementsReportColumnNames.quantity_quality)),
    )
    return df
