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

using Energinet.DataHub.SettlementReport.Infrastructure.SqlStatements.DeltaTableConstants;

namespace Energinet.DataHub.Wholesale.CalculationResults.IntegrationTests.Fixtures;

public class WholesaleResultsTableSchemaDefinition
{
    /// <summary>
    /// The schema definition of the table expressed as (Column name, Data type, Is nullable).
    /// </summary>
    public static Dictionary<string, (string DataType, bool IsNullable)> SchemaDefinition => new()
    {
        { WholesaleResultColumnNames.CalculationId, ("string", false) },
        { WholesaleResultColumnNames.CalculationType, ("string", false) },
        { WholesaleResultColumnNames.CalculationExecutionTimeStart, ("timestamp", false) },
        { WholesaleResultColumnNames.CalculationResultId, ("string", false) },
        { WholesaleResultColumnNames.GridArea, ("string", false) },
        { WholesaleResultColumnNames.EnergySupplierId, ("string", false) },
        { WholesaleResultColumnNames.Quantity, ("decimal(18,3)", true) },
        { WholesaleResultColumnNames.QuantityUnit, ("string", false) },
        { WholesaleResultColumnNames.QuantityQualities, ("array<string>", true) },
        { WholesaleResultColumnNames.Time, ("timestamp", false) },
        { WholesaleResultColumnNames.Resolution, ("string", false) },
        { WholesaleResultColumnNames.MeteringPointType, ("string", true) },
        { WholesaleResultColumnNames.SettlementMethod, ("string", true) },
        { WholesaleResultColumnNames.Price, ("decimal(18,6)", true) },
        { WholesaleResultColumnNames.Amount, ("decimal(18,6)", false) },
        { WholesaleResultColumnNames.IsTax, ("boolean", false) },
        { WholesaleResultColumnNames.ChargeCode, ("string", false) },
        { WholesaleResultColumnNames.ChargeType, ("string", false) },
        { WholesaleResultColumnNames.ChargeOwnerId, ("string", false) },
        { WholesaleResultColumnNames.AmountType, ("string", false) },
    };
}