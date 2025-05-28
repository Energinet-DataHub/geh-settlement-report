using Energinet.DataHub.Reports.Application.SettlementReports_v2;
using Energinet.DataHub.Reports.Interfaces.Helpers;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.MeasurementsReport;
using NodaTime;

namespace Energinet.DataHub.Reports.Application.MeasurementsReport.Services;

public sealed class ListMeasurementsReportService : IListMeasurementsReportService
{
    private readonly IMeasurementsReportService _measurementsReportService;
    private readonly ISettlementReportDatabricksJobsHelper _jobHelper;
    private readonly IMeasurementsReportRepository _repository;
    private readonly IClock _clock;

    public ListMeasurementsReportService(IMeasurementsReportService measurementsReportService, ISettlementReportDatabricksJobsHelper jobHelper, IMeasurementsReportRepository repository, IClock clock)
    {
        _measurementsReportService = measurementsReportService;
        _jobHelper = jobHelper;
        _repository = repository;
        _clock = clock;
    }

    public Task<IEnumerable<RequestedMeasurementsReportDto>> GetAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<RequestedMeasurementsReportDto>> GetAsync(Guid actorId)
    {
        var settlementReports = await _measurementsReportService.GetReportsAsync(actorId).ConfigureAwait(false);
        return await GetMeasurementsReportsAsync(settlementReports).ConfigureAwait(false);
    }

    private async Task<IEnumerable<RequestedMeasurementsReportDto>> GetMeasurementsReportsAsync(IEnumerable<RequestedMeasurementsReportDto> measurementsReports)
    {
        var results = new List<RequestedMeasurementsReportDto>();
        foreach (var measurementsReportDto in measurementsReports)
        {
            if (measurementsReportDto.Status != ReportStatus.Completed)
            {
                var jobResult = await _jobHelper.GetJobRunAsync(measurementsReportDto.JobRunId!.Id)
                    .ConfigureAwait(false);
                switch (jobResult.Status)
                {
                    case JobRunStatus.Completed:
                        await MarkAsCompletedAsync(measurementsReportDto, jobResult.EndTime).ConfigureAwait(false);
                        break;
                    case JobRunStatus.Canceled:
                        await MarkAsCanceledAsync(measurementsReportDto).ConfigureAwait(false);
                        break;
                    case JobRunStatus.Failed:
                        await MarkAsFailedAsync(measurementsReportDto).ConfigureAwait(false);
                        break;
                }

                results.Add(measurementsReportDto with { Status = MapFromJobStatus(jobResult.Status) });
            }
            else
            {
                results.Add(measurementsReportDto);
            }
        }

        return results;
    }

    private async Task MarkAsCompletedAsync(RequestedMeasurementsReportDto measurementsReportDto, DateTimeOffset? endTime)
    {
        ArgumentNullException.ThrowIfNull(measurementsReportDto);
        ArgumentNullException.ThrowIfNull(measurementsReportDto.JobRunId);

        var request = await _repository
            .GetByJobRunIdAsync(measurementsReportDto.JobRunId.Id)
            .ConfigureAwait(false);

        request.MarkAsCompleted(_clock, measurementsReportDto.RequestId, endTime);

        await _repository
            .AddOrUpdateAsync(request)
            .ConfigureAwait(false);
    }

    private async Task MarkAsFailedAsync(RequestedMeasurementsReportDto measurementsReportDto)
    {
        ArgumentNullException.ThrowIfNull(measurementsReportDto);
        ArgumentNullException.ThrowIfNull(measurementsReportDto.JobRunId);

        var request = await _repository
            .GetByJobRunIdAsync(measurementsReportDto.JobRunId.Id)
            .ConfigureAwait(false);

        request.MarkAsFailed();

        await _repository
            .AddOrUpdateAsync(request)
            .ConfigureAwait(false);
    }

    private async Task MarkAsCanceledAsync(RequestedMeasurementsReportDto measurementsReportDto)
    {
        ArgumentNullException.ThrowIfNull(measurementsReportDto);
        ArgumentNullException.ThrowIfNull(measurementsReportDto.JobRunId);

        var request = await _repository
            .GetByJobRunIdAsync(measurementsReportDto.JobRunId.Id)
            .ConfigureAwait(false);

        request.MarkAsCanceled();

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
