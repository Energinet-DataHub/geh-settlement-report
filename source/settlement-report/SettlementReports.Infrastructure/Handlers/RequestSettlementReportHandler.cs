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

using Energinet.DataHub.SettlementReport.Infrastructure.Helpers;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.SettlementReport.Infrastructure.Handlers;

public sealed class RequestSettlementReportHandler : IRequestSettlemenReportJobHandler
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

    public async Task<long> HandleAsync(SettlementReportRequestDto request, Guid userId, Guid actorId, bool isFas)
    {
        var jobId = await _jobHelper.RunSettlementReportsJobAsync(request).ConfigureAwait(false);
        await _settlementReportInitializeHandler
            .InitializeFromJobAsync(
                userId,
                actorId,
                isFas,
                jobId,
                request)
            .ConfigureAwait(false);

        return jobId;
    }
}
