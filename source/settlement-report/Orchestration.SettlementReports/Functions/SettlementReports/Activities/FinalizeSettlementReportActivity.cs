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

using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.SettlementReport;
using Microsoft.Azure.Functions.Worker;

namespace Energinet.DataHub.SettlementReport.Orchestration.SettlementReports.Functions.SettlementReports.Activities;

public sealed class FinalizeSettlementReportActivity
{
    private readonly ISettlementReportFinalizeHandler _settlementReportFinalizeHandler;

    public FinalizeSettlementReportActivity(ISettlementReportFinalizeHandler settlementReportFinalizeHandler)
    {
        _settlementReportFinalizeHandler = settlementReportFinalizeHandler;
    }

    [Function(nameof(FinalizeSettlementReportActivity))]
    public Task Run([ActivityTrigger] GeneratedSettlementReportDto input)
    {
        return _settlementReportFinalizeHandler.FinalizeAsync(input);
    }
}
