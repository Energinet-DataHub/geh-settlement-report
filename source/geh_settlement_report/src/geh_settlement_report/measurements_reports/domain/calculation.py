import shutil
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
    MeasurementsGoldCurrentV1ColumnNames,
    MeasurementsReportColumnNames,
    MeteringPointPeriodsColumnNames,
)
from geh_settlement_report.measurements_reports.domain.file_name_factory import file_name_factory


def execute(
    spark: SparkSession,
    args: MeasurementsReportArgs,
    calculated_measurements: DataFrame,
    metering_point_periods: DataFrame,
) -> DataFrame:
    filtered_measurements = _filter_calculated_measurements(args, calculated_measurements)
    filtered_metering_point_periods = _filter_metering_point_periods(args, metering_point_periods)

    result = (
        filtered_measurements.alias("m")
        .join(
            filtered_metering_point_periods.alias("p"),
            on=[
                F.col(f"m.{MeasurementsGoldCurrentV1ColumnNames.metering_point_id}")
                == F.col(f"p.{MeteringPointPeriodsColumnNames.metering_point_id}"),
                F.col(f"m.{MeasurementsGoldCurrentV1ColumnNames.observation_time}")
                >= F.col(f"p.{MeteringPointPeriodsColumnNames.period_from_date}"),
                F.col(f"m.{MeasurementsGoldCurrentV1ColumnNames.observation_time}")
                < F.coalesce(
                    F.col(f"p.{MeteringPointPeriodsColumnNames.period_to_date}"),
                    F.lit("9999-12-31 23:59:59.999999").cast("timestamp"),
                ),
            ],
            how="inner",
        )
        .select(
            F.col(f"p.{MeteringPointPeriodsColumnNames.grid_area_code}").alias(
                MeasurementsReportColumnNames.grid_area_code
            ),
            F.col(f"p.{MeteringPointPeriodsColumnNames.metering_point_id}").alias(
                MeasurementsReportColumnNames.metering_point_id
            ),
            F.col(f"p.{MeteringPointPeriodsColumnNames.metering_point_type}").alias(
                MeasurementsReportColumnNames.metering_point_type
            ),
            F.col(f"p.{MeteringPointPeriodsColumnNames.resolution}").alias(MeasurementsReportColumnNames.resolution),
            F.col(f"p.{MeteringPointPeriodsColumnNames.energy_supplier_id}").alias(
                MeasurementsReportColumnNames.energy_supplier_id
            ),
            F.col(f"p.{MeteringPointPeriodsColumnNames.physical_status}").alias(
                MeasurementsReportColumnNames.physical_status
            ),
            F.col(f"m.{MeasurementsGoldCurrentV1ColumnNames.observation_time}").alias(
                MeasurementsReportColumnNames.observation_time
            ),
            F.col(f"m.{MeasurementsGoldCurrentV1ColumnNames.quantity}").alias(MeasurementsReportColumnNames.quantity),
            F.col(f"m.{MeasurementsGoldCurrentV1ColumnNames.quality}").alias(
                MeasurementsReportColumnNames.quantity_quality
            ),
            F.col(f"p.{MeteringPointPeriodsColumnNames.quantity_unit}").alias(MeasurementsReportColumnNames.unit),
        )
    )

    report_output_path = Path(args.output_path) / args.report_id
    tmp_dir = Path(args.output_path) / "tmp"
    dbutils = get_dbutils(spark)

    files = write_csv_files(
        df=result,
        dbutils=dbutils,
        output_path=report_output_path.as_posix(),
        tmpdir=tmp_dir.as_posix(),
        file_name_factory=lambda *_: f"{file_name_factory(args)}.csv",
    )

    # create_zip_file(
    #     dbutils,
    #     report_output_path.with_suffix(".zip").as_posix(),
    #     [f.as_posix() for f in dbutils.fs.ls(report_output_path.as_posix())],
    # )

    create_zip_file(
        dbutils,
        report_output_path.with_suffix(".zip").as_posix(),
        [f.as_posix() for f in files],
    )

    shutil.rmtree(tmp_dir, ignore_errors=True)

    return result


def _filter_metering_point_periods(args: MeasurementsReportArgs, df: DataFrame) -> DataFrame:
    df_with_grid_area_codes = df.filter(
        F.col(MeteringPointPeriodsColumnNames.grid_area_code).isin(args.grid_area_codes)
        | F.col(MeteringPointPeriodsColumnNames.from_grid_area_code).isin(args.grid_area_codes)
        | F.col(MeteringPointPeriodsColumnNames.to_grid_area_code).isin(args.grid_area_codes)
    )

    if args.energy_supplier_ids is not None:
        df_with_grid_area_codes = df_with_grid_area_codes.filter(
            F.col(MeteringPointPeriodsColumnNames.energy_supplier_id).isin(args.energy_supplier_ids)
        )

    df_within_period = df_with_grid_area_codes.filter(
        (F.col(MeteringPointPeriodsColumnNames.period_from_date) < F.lit(args.period_end))
        & (
            F.coalesce(
                F.col(MeteringPointPeriodsColumnNames.period_to_date),
                F.lit("9999-12-31 23:59:59.999999").cast("timestamp"),
            )
            > F.lit(args.period_start)
        )
    )

    return df_within_period


def _filter_calculated_measurements(args: MeasurementsReportArgs, df: DataFrame) -> DataFrame:
    df_in_period = df.filter(
        F.col(MeasurementsGoldCurrentV1ColumnNames.observation_time).between(args.period_start, args.period_end)
    )
    return df_in_period
