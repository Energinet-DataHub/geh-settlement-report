// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.Reports.SubsystemTests.Fixtures;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Reports.SubsystemTests.Features.SettlementReport.Fixtures;

public class SettlementReportSubsystemTestConfiguration : ReportsSubsystemTestConfiguration
{
    public SettlementReportSubsystemTestConfiguration()
    {
        var calculationId = Root.GetValue<string>("EXISTING_WHOLESALE_FIXING_CALCULATION_ID") ?? throw new NullReferenceException($"Missing configuration value for EXISTING_WHOLESALE_FIXING_CALCULATION_ID");

        ExistingWholesaleFixingCalculationId = new CalculationId(new Guid(calculationId));
    }

    public CalculationId ExistingWholesaleFixingCalculationId { get; }
}
