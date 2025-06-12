import shutil
from itertools import chain
from pathlib import Path
from zoneinfo import ZoneInfo

from geh_common.databricks.get_dbutils import get_dbutils
from geh_common.infrastructure.create_zip import create_zip_file
from geh_common.infrastructure.write_csv import write_csv_files
from geh_common.pyspark.transformations import convert_from_utc
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
    calculated_measurements = convert_from_utc(calculated_measurements, args.time_zone)
    metering_point_periods = convert_from_utc(metering_point_periods, args.time_zone)

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
            F.col(f"p.{MeteringPointPeriodsColumnNames.quantity_unit}").alias(MeasurementsReportColumnNames.unit),
            F.col(f"p.{MeteringPointPeriodsColumnNames.physical_status}").alias(
                MeasurementsReportColumnNames.physical_status
            ),
            F.col(f"m.{MeasurementsGoldCurrentV1ColumnNames.quality}").alias(
                MeasurementsReportColumnNames.quantity_quality
            ),
            F.col(f"m.{MeasurementsGoldCurrentV1ColumnNames.observation_time}").alias(
                MeasurementsReportColumnNames.observation_time
            ),
            F.col(f"m.{MeasurementsGoldCurrentV1ColumnNames.quantity}").alias(MeasurementsReportColumnNames.quantity),
        )
    )

    # Add metering point type mapping
    metering_point_type_mapping = {
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
    }

    # Map the metering point type values
    result = _map_output_report_column(
        MeasurementsReportColumnNames.metering_point_type, metering_point_type_mapping, result
    )

    # Add unit mapping
    unit_mapping = {
        "kWh": "kWh",
        "kVArh": "kVArh",
        "TNE": "Tonne",
    }

    # Map the unit values
    result = _map_output_report_column(MeasurementsReportColumnNames.unit, unit_mapping, result)

    # Add physical status mapping
    physical_status_mapping = {"connected": "E22", "disconnected": "E23"}

    # Map the physical status values
    result = _map_output_report_column(MeasurementsReportColumnNames.physical_status, physical_status_mapping, result)

    # Add quality mapping
    quality_mapping = {
        "measured": "Measured",
        "missing": "Missing",
        "estimated": "Estimated",
        "calculated": "Calculated",
    }

    # Map the quality values
    result = _map_output_report_column(MeasurementsReportColumnNames.quantity_quality, quality_mapping, result)

    # Format the observation_time column to dd-MM-yyyy HH:mm format
    result = result.withColumn(
        MeasurementsReportColumnNames.observation_time,
        F.date_format(F.col(MeasurementsReportColumnNames.observation_time), "dd-MM-yyyy HH:mm"),
    )

    report_output_path = Path(args.output_path) / args.report_id
    tmp_dir = report_output_path / "tmp"
    dbutils = get_dbutils(spark)

    files = write_csv_files(
        df=result,
        output_path=report_output_path.as_posix(),
        tmpdir=tmp_dir.as_posix(),
        file_name_factory=lambda *_: f"{file_name_factory(args)}.csv",
    )

    create_zip_file(
        dbutils,
        report_output_path.with_suffix(".zip").as_posix(),
        [f.as_posix() for f in files],
    )

    shutil.rmtree(report_output_path, ignore_errors=True)

    return result


def _map_output_report_column(column_name: str, mapping_data: dict, result: DataFrame) -> DataFrame:
    mapping_expr = F.create_map([F.lit(x) for x in chain(*[(k, v) for k, v in mapping_data.items()])])
    return result.withColumn(
        column_name,
        mapping_expr.getItem(F.col(column_name)),
    )


def _filter_metering_point_periods(args: MeasurementsReportArgs, df: DataFrame) -> DataFrame:
    period_start_local = args.period_start.astimezone(ZoneInfo(args.time_zone))
    period_end_local = args.period_end.astimezone(ZoneInfo(args.time_zone))

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
        (F.col(MeteringPointPeriodsColumnNames.period_from_date) < F.lit(period_end_local))
        & (
            F.coalesce(
                F.col(MeteringPointPeriodsColumnNames.period_to_date),
                F.lit("9999-12-31 23:59:59.999999").cast("timestamp"),
            )
            > F.lit(period_start_local)
        )
    )

    return df_within_period


def _filter_calculated_measurements(args: MeasurementsReportArgs, df: DataFrame) -> DataFrame:
    period_start_local = args.period_start.astimezone(ZoneInfo(args.time_zone))
    period_end_local = args.period_end.astimezone(ZoneInfo(args.time_zone))
    df_in_period = df.filter(
        (F.col(MeasurementsGoldCurrentV1ColumnNames.observation_time) >= period_start_local)
        & (F.col(MeasurementsGoldCurrentV1ColumnNames.observation_time) < period_end_local)
    )
    return df_in_period
