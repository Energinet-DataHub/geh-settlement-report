using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.SettlementReport;
using Energinet.DataHub.Reports.Infrastructure.Helpers;
using NodaTime;

namespace Energinet.DataHub.Reports.Application.SettlementReports.Handlers;

public sealed class ListSettlementReportJobsHandler : IListSettlementReportJobsHandler
{
    private readonly ISettlementReportDatabricksJobsHelper _jobHelper;
    private readonly IGetSettlementReportsHandler _getSettlementReportsHandler;
    private readonly ISettlementReportRepository _repository;
    private readonly IClock _clock;

    public ListSettlementReportJobsHandler(
        ISettlementReportDatabricksJobsHelper jobHelper,
        IGetSettlementReportsHandler getSettlementReportsHandler,
        ISettlementReportRepository repository,
        IClock clock)
    {
        _jobHelper = jobHelper;
        _getSettlementReportsHandler = getSettlementReportsHandler;
        _repository = repository;
        _clock = clock;
    }

    public async Task<IEnumerable<RequestedSettlementReportDto>> HandleAsync()
    {
        var settlementReports = await _getSettlementReportsHandler.GetForJobsAsync().ConfigureAwait(false);
        return await GetSettlementReportsAsync(settlementReports).ConfigureAwait(false);
    }

    public async Task<IEnumerable<RequestedSettlementReportDto>> HandleAsync(Guid actorId)
    {
        var settlementReports = await _getSettlementReportsHandler.GetForJobsAsync(actorId).ConfigureAwait(false);
        return await GetSettlementReportsAsync(settlementReports).ConfigureAwait(false);
    }

    private async Task<IEnumerable<RequestedSettlementReportDto>> GetSettlementReportsAsync(IEnumerable<RequestedSettlementReportDto> settlementReports)
    {
        var results = new List<RequestedSettlementReportDto>();
        foreach (var settlementReportDto in settlementReports)
        {
            if (settlementReportDto.Status != ReportStatus.Completed)
            {
                var jobResult = await _jobHelper.GetJobRunAsync(settlementReportDto.JobId!.Id)
                    .ConfigureAwait(false);
                switch (jobResult.Status)
                {
                    case JobRunStatus.Completed:
                        await MarkAsCompletedAsync(settlementReportDto, jobResult.EndTime).ConfigureAwait(false);
                        break;
                    case JobRunStatus.Canceled:
                        await MarkAsCanceledAsync(settlementReportDto).ConfigureAwait(false);
                        break;
                    case JobRunStatus.Failed:
                        await MarkAsFailedAsync(settlementReportDto).ConfigureAwait(false);
                        break;
                }

                results.Add(settlementReportDto with { Status = MapFromJobStatus(jobResult.Status) });
            }
            else
            {
                results.Add(settlementReportDto);
            }
        }

        return results;
    }

    private async Task MarkAsCompletedAsync(RequestedSettlementReportDto settlementReportDto, DateTimeOffset? endTime)
    {
        ArgumentNullException.ThrowIfNull(settlementReportDto);
        ArgumentNullException.ThrowIfNull(settlementReportDto.JobId);

        var request = await _repository
            .GetAsync(settlementReportDto.JobId.Id)
            .ConfigureAwait(false);

        request.MarkAsCompleted(_clock, settlementReportDto.RequestId, endTime);

        await _repository
            .AddOrUpdateAsync(request)
            .ConfigureAwait(false);
    }

    private async Task MarkAsCanceledAsync(RequestedSettlementReportDto settlementReportDto)
    {
        ArgumentNullException.ThrowIfNull(settlementReportDto);
        ArgumentNullException.ThrowIfNull(settlementReportDto.JobId);

        var request = await _repository
            .GetAsync(settlementReportDto.JobId.Id)
            .ConfigureAwait(false);

        request.MarkAsCanceled();

        await _repository
            .AddOrUpdateAsync(request)
            .ConfigureAwait(false);
    }

    private async Task MarkAsFailedAsync(RequestedSettlementReportDto settlementReportDto)
    {
        ArgumentNullException.ThrowIfNull(settlementReportDto);
        ArgumentNullException.ThrowIfNull(settlementReportDto.JobId);

        var request = await _repository
            .GetAsync(settlementReportDto.JobId.Id)
            .ConfigureAwait(false);

        request.MarkAsFailed();

        await _repository
            .AddOrUpdateAsync(request)
            .ConfigureAwait(false);
    }

    private ReportStatus MapFromJobStatus(JobRunStatus status)
    {
        return status switch
        {
            JobRunStatus.Running => ReportStatus.InProgress,
            JobRunStatus.Queued => ReportStatus.InProgress,
            JobRunStatus.Completed => ReportStatus.Completed,
            JobRunStatus.Canceled => ReportStatus.Canceled,
            JobRunStatus.Failed => ReportStatus.Failed,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
        };
    }
}
