from pyspark.sql import DataFrame
from pyspark.sql import functions as F

from geh_settlement_report.measurements_reports.application.job_args.measurements_report_args import (
    MeasurementsReportArgs,
)


def execute(
    args: MeasurementsReportArgs, calculated_measurements: DataFrame, metering_point_periods: DataFrame
) -> DataFrame:
    filtered_measurements = apply_filters(args, calculated_measurements)
    filtered_metering_point_periods = apply_filters(args, metering_point_periods)

    result = (
        filtered_measurements.alias("m")
        .join(
            filtered_metering_point_periods.alias("p"),
            on=[
                F.col("m.metering_point_id") == F.col("p.metering_point_id"),
                F.col("m.observation_time") >= F.col("p.period_from_date"),
                F.col("m.observation_time")
                < F.coalesce(F.col("p.period_to_date"), F.lit("9999-12-31 23:59:59.999999").cast("timestamp")),
            ],
            how="inner",
        )
        .select(
            F.col("p.grid_area_code").alias("grid_area_code"),
            F.col("p.metering_point_id").alias("metering_point_id"),
            F.col("p.metering_point_type").alias("metering_point_type"),
            F.col("p.resolution").alias("resolution"),
            F.col("p.energy_supplier_id").alias("energy_supplier_id"),
            F.col("p.physical_status").alias("physical_status"),
            F.col("m.observation_time").alias("observation_time"),
            F.col("m.quantity").alias("quantity"),
            F.col("m.quality").alias("quantity_quality"),
            F.col("p.quantity_unit").alias("unit"),
        )
    )

    result.show()

    return result


def apply_filters(args: MeasurementsReportArgs, df: DataFrame) -> DataFrame:
    filtered = df
    if (
        "grid_area_code" in filtered.columns
        or "from_grid_area_code" in filtered.columns
        or "to_grid_area_code" in filtered.columns
    ):
        filtered = filtered.filter(
            F.col("grid_area_code").isin(args.grid_area_codes)
            | F.col("from_grid_area_code").isin(args.grid_area_codes)
            | F.col("to_grid_area_code").isin(args.grid_area_codes)
        )
    if "observation_time" in filtered.columns:
        filtered = filtered.filter(
            (F.col("observation_time") >= args.period_start) & (F.col("observation_time") < args.period_end)
        )
    return filtered


def join_to_result(calculated_measurements: DataFrame, metering_point_periods: DataFrame):
    pass
