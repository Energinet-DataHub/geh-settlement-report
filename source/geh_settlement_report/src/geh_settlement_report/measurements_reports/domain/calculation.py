from pathlib import Path

from geh_common.databricks.get_dbutils import get_dbutils
from geh_common.infrastructure.create_zip import create_zip_file
from geh_common.infrastructure.write_csv import write_csv_files
from pyspark.sql import DataFrame, SparkSession
from pyspark.sql import functions as F

from geh_settlement_report.measurements_reports.application.job_args.measurements_report_args import (
    MeasurementsReportArgs,
)
from geh_settlement_report.measurements_reports.domain.column_names import (
    MeasurementsGoldCurrentV1,
    MeasurementsReport,
    MeteringPointPeriods,
)
from geh_settlement_report.measurements_reports.domain.file_name_factory import file_name_factory


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
                F.col(f"m.{MeasurementsGoldCurrentV1.metering_point_id}")
                == F.col(f"p.{MeteringPointPeriods.metering_point_id}"),
                F.col(f"m.{MeasurementsGoldCurrentV1.observation_time}")
                >= F.col(f"p.{MeteringPointPeriods.period_from_date}"),
                F.col(f"m.{MeasurementsGoldCurrentV1.observation_time}")
                < F.coalesce(
                    F.col(f"p.{MeteringPointPeriods.period_to_date}"),
                    F.lit("9999-12-31 23:59:59.999999").cast("timestamp"),
                ),
            ],
            how="inner",
        )
        .select(
            F.col(f"p.{MeteringPointPeriods.grid_area_code}").alias(MeasurementsReport.grid_area_code),
            F.col(f"p.{MeteringPointPeriods.metering_point_id}").alias(MeasurementsReport.metering_point_id),
            F.col(f"p.{MeteringPointPeriods.metering_point_type}").alias(MeasurementsReport.metering_point_type),
            F.col(f"p.{MeteringPointPeriods.resolution}").alias(MeasurementsReport.resolution),
            F.col(f"p.{MeteringPointPeriods.energy_supplier_id}").alias(MeasurementsReport.energy_supplier_id),
            F.col(f"p.{MeteringPointPeriods.physical_status}").alias(MeasurementsReport.physical_status),
            F.col(f"m.{MeasurementsGoldCurrentV1.observation_time}").alias(MeasurementsReport.observation_time),
            F.col(f"m.{MeasurementsGoldCurrentV1.quantity}").alias(MeasurementsReport.quantity),
            F.col(f"m.{MeasurementsGoldCurrentV1.quality}").alias(MeasurementsReport.quantity_quality),
            F.col(f"p.{MeteringPointPeriods.quantity_unit}").alias(MeasurementsReport.unit),
        )
    )

    files = write_csv_files(
        result,
        args.output_path,
        file_name_factory=lambda *_: f"{file_name_factory(args)}.csv",
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
        MeteringPointPeriods.grid_area_code in filtered.columns
        or MeteringPointPeriods.from_grid_area_code in filtered.columns
        or MeteringPointPeriods.to_grid_area_code in filtered.columns
    ):
        filtered = filtered.filter(
            F.col(MeteringPointPeriods.grid_area_code).isin(args.grid_area_codes)
            | F.col(MeteringPointPeriods.grid_area_code).isin(args.grid_area_codes)
            | F.col(MeteringPointPeriods.grid_area_code).isin(args.grid_area_codes)
        )
    if MeasurementsGoldCurrentV1.observation_time in filtered.columns:
        filtered = filtered.filter(
            (F.col(MeasurementsGoldCurrentV1.observation_time) >= args.period_start)
            & (F.col(MeasurementsGoldCurrentV1.observation_time) < args.period_end)
        )
    return filtered
