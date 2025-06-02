from pathlib import Path

from geh_common.databricks.get_dbutils import get_dbutils
from geh_common.infrastructure.create_zip import create_zip_file
from geh_common.infrastructure.write_csv import write_csv_files
from pyspark.sql import DataFrame, SparkSession
from pyspark.sql import functions as F

from geh_settlement_report.measurements_reports.application.job_args.measurements_report_args import (
    MeasurementsReportArgs,
)


def execute(
    spark: SparkSession,
    args: MeasurementsReportArgs,
    calculated_measurements: DataFrame,
    metering_point_periods: DataFrame,
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

    def file_name_generater(_: str, partitions: dict) -> str:
        """Generate a file name based on the provided file name and partitions.

        This function is used to create a unique file name for each partitioned file.
        """
        return (
            f"measurements_report_{args.period_start.strftime('%d-%m-%Y')}_{args.period_end.strftime('%d-%m-%Y')}.csv"
        )

    files = write_csv_files(
        result,
        args.output_path,
        file_name_factory=file_name_generater,
    )
    create_zip_file(
        get_dbutils(spark),
        Path(args.output_path) / f"{args.report_id}.zip",
        [f.as_posix() for f in files],
    )

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
