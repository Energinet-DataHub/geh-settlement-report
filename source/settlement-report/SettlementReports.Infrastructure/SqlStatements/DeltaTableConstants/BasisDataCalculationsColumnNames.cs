﻿// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Energinet.DataHub.SettlementReport.Infrastructure.SqlStatements.DeltaTableConstants;

public class BasisDataCalculationsColumnNames
{
    public const string CalculationId = "calculation_id";
    public const string CalculationType = "calculation_type";
    public const string PeriodStart = "period_start";
    public const string PeriodEnd = "period_end";
    public const string ExecutionTimeStart = "execution_time_start";
    public const string CreatedByUserId = "created_by_user_id";
    public const string Version = "version";
}