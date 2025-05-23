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

using Energinet.DataHub.SettlementReport.Application.Model;
using Energinet.DataHub.SettlementReport.Application.Services;
using Energinet.DataHub.SettlementReport.Application.SettlementReports.Commands;
using Energinet.DataHub.SettlementReport.Interfaces.Helpers;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.SettlementReport;
using NodaTime.Extensions;

namespace Energinet.DataHub.SettlementReport.Application.SettlementReports.Handlers;

public sealed class RequestSettlementReportJobHandler : IRequestSettlementReportJobHandler
{
    private readonly IDatabricksJobsHelper _jobHelper;
    private readonly ISettlementReportInitializeHandler _settlementReportInitializeHandler;
    private readonly IGridAreaOwnerRepository _gridAreaOwnerRepository;

    public RequestSettlementReportJobHandler(
        IDatabricksJobsHelper jobHelper,
        ISettlementReportInitializeHandler settlementReportInitializeHandler,
        IGridAreaOwnerRepository gridAreaOwnerRepository)
    {
        _jobHelper = jobHelper;
        _settlementReportInitializeHandler = settlementReportInitializeHandler;
        _gridAreaOwnerRepository = gridAreaOwnerRepository;
    }

    public async Task<JobRunId> HandleAsync(RequestSettlementReportCommand request)
    {
        if (request.MarketRole == MarketRole.GridAccessProvider && request.RequestDto.Filter.GridAreas.Count > 0)
        {
            JobRunId? firstRunId = null;

            var gridAreas = request.RequestDto.Filter.GridAreas.Select(x => x.Key);

            var distinctOwners = await GetGridAreaOwnersAsync(gridAreas, request.RequestDto.Filter).ConfigureAwait(false);

            foreach (var owner in distinctOwners)
            {
                var id = await StartReportAsync(request, owner.Value).ConfigureAwait(false);
                firstRunId ??= id;
            }

            return firstRunId!;
        }

        return await StartReportAsync(request, request.ActorGln).ConfigureAwait(false);
    }

    private async Task<IEnumerable<ActorNumber>> GetGridAreaOwnersAsync(IEnumerable<string> gridAreaCodes, SettlementReportRequestFilterDto filter)
    {
        var gridAreaOwners = new HashSet<ActorNumber>();

        foreach (var grid in gridAreaCodes)
        {
            var owners = await _gridAreaOwnerRepository.GetGridAreaOwnersAsync(
                new GridAreaCode(grid),
                filter.PeriodStart.ToInstant(),
                filter.PeriodEnd.ToInstant()).ConfigureAwait(false);

            foreach (var owner in owners)
            {
                gridAreaOwners.Add(owner.ActorNumber);
            }
        }

        return gridAreaOwners;
    }

    private async Task<JobRunId> StartReportAsync(RequestSettlementReportCommand request, string requestActorGln)
    {
        var reportId = new ReportRequestId(Guid.NewGuid().ToString());

        var runId = await _jobHelper.RunJobAsync(request.RequestDto, request.MarketRole, reportId, requestActorGln).ConfigureAwait(false);

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
