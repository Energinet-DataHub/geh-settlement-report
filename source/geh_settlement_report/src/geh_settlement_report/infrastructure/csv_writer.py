# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
import os
from dataclasses import dataclass
from pathlib import Path
from typing import Any

from geh_common.infrastructure.write_csv import write_csv_files
from geh_common.telemetry import Logger, use_span
from pyspark.sql import DataFrame

from geh_settlement_report.domain.utils.csv_column_names import EphemeralColumns
from geh_settlement_report.domain.utils.report_data_type import ReportDataType
from geh_settlement_report.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from geh_settlement_report.infrastructure.paths import get_report_output_path
from geh_settlement_report.infrastructure.report_name_factory import FileNameFactory

log = Logger(__name__)


@dataclass
class TmpFile:
    src: Path
    dst: Path
    tmp_dst: Path


@use_span()
def write(
    dbutils: Any,
    args: SettlementReportArgs,
    df: DataFrame,
    report_data_type: ReportDataType,
    order_by_columns: list[str],
    rows_per_file: int = 1_000_000,
) -> list[str]:
    report_output_path = get_report_output_path(args)

    partition_columns = []
    if EphemeralColumns.grid_area_code_partitioning in df.columns:
        partition_columns.append(EphemeralColumns.grid_area_code_partitioning)

    file_name_factory = FileNameFactory(report_data_type, args)

    files_paths = write_csv_files(
        df=df,
        output_path=report_output_path,
        spark_output_path=f"{report_output_path}/{_get_folder_name(report_data_type)}",
        rows_per_file=rows_per_file if args.prevent_large_text_files else None,
        partition_columns=partition_columns,
        order_by=order_by_columns,
        file_name_factory=file_name_factory.create,
    )

    file_names = [os.path.basename(file_path) for file_path in files_paths]

    return file_names


def _get_folder_name(report_data_type: ReportDataType) -> str:
    if report_data_type == ReportDataType.TimeSeriesHourly:
        return "time_series_points_hourly"
    elif report_data_type == ReportDataType.TimeSeriesQuarterly:
        return "time_series_points_quarterly"
    elif report_data_type == ReportDataType.MeteringPointPeriods:
        return "metering_point_periods"
    elif report_data_type == ReportDataType.ChargeLinks:
        return "charge_link_periods"
    elif report_data_type == ReportDataType.ChargePricePoints:
        return "charge_price_points"
    elif report_data_type == ReportDataType.EnergyResults:
        return "energy_results"
    elif report_data_type == ReportDataType.MonthlyAmounts:
        return "monthly_amounts"
    elif report_data_type == ReportDataType.WholesaleResults:
        return "wholesale_results"
    else:
        raise ValueError(f"Unsupported report data type: {report_data_type}")
