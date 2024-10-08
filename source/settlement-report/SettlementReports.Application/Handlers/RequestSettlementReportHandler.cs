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

using Energinet.DataHub.SettlementReport.Application.Commands;
using Energinet.DataHub.SettlementReport.Interfaces.Helpers;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.SettlementReport.Application.Handlers;

public sealed class RequestSettlementReportHandler : IRequestSettlementReportJobHandler
{
    private readonly IDatabricksJobsHelper _jobHelper;
    private readonly ISettlementReportInitializeHandler _settlementReportInitializeHandler;

    public RequestSettlementReportHandler(
        IDatabricksJobsHelper jobHelper,
        ISettlementReportInitializeHandler settlementReportInitializeHandler)
    {
        _jobHelper = jobHelper;
        _settlementReportInitializeHandler = settlementReportInitializeHandler;
    }

    public async Task<JobRunId> HandleAsync(RequestSettlementReportCommand request)
    {
        var reportId = new SettlementReportRequestId(Guid.NewGuid().ToString());
        var runId = await _jobHelper.RunSettlementReportsJobAsync(request.RequestDto, request.MarketRole, reportId, request.ActorGln).ConfigureAwait(false);
        await _settlementReportInitializeHandler
            .InitializeFromJobAsync(
                request.UserId,
                request.ActorId,
                request.IsFas,
                runId,
                reportId,
                request.RequestDto)
            .ConfigureAwait(false);

        return runId;
    }
}
