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

from pyspark.sql import DataFrame

from geh_settlement_report.settlement_reports.application.job_args.settlement_report_args import (
    SettlementReportArgs,
)
from geh_settlement_report.settlement_reports.domain.charge_price_points.prepare_for_csv import (
    prepare_for_csv,
)
from geh_settlement_report.settlement_reports.domain.charge_price_points.read_and_filter import (
    read_and_filter,
)
from geh_settlement_report.settlement_reports.infrastructure.repository import WholesaleRepository


def create_charge_price_points(
    args: SettlementReportArgs,
    repository: WholesaleRepository,
) -> DataFrame:
    charge_price_points = read_and_filter(
        args.period_start,
        args.period_end,
        args.calculation_id_by_grid_area,
        args.energy_supplier_ids,
        args.requesting_actor_market_role,
        args.requesting_actor_id,
        repository,
    )

    return prepare_for_csv(charge_price_points, args.time_zone)
