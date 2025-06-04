using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.SettlementReport;
using Energinet.DataHub.Reports.Application.Model;
using Energinet.DataHub.Reports.Application.Services;
using Energinet.DataHub.Reports.Application.SettlementReports.Commands;
using Energinet.DataHub.Reports.Infrastructure.Helpers;
using NodaTime.Extensions;

namespace Energinet.DataHub.Reports.Application.SettlementReports.Handlers;

public sealed class RequestSettlementReportJobHandler : IRequestSettlementReportJobHandler
{
    private readonly ISettlementReportDatabricksJobsHelper _jobHelper;
    private readonly ISettlementReportPersistenceService _settlementReportPersistenceService;
    private readonly IGridAreaOwnerRepository _gridAreaOwnerRepository;

    public RequestSettlementReportJobHandler(
        ISettlementReportDatabricksJobsHelper jobHelper,
        ISettlementReportPersistenceService settlementReportPersistenceService,
        IGridAreaOwnerRepository gridAreaOwnerRepository)
    {
        _jobHelper = jobHelper;
        _settlementReportPersistenceService = settlementReportPersistenceService;
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

        await _settlementReportPersistenceService
            .PersistAsync(
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
