using Energinet.DataHub.Reports.Application.MeasurementsReport.Commands;
using Energinet.DataHub.Reports.Interfaces.Helpers;
using Energinet.DataHub.Reports.Interfaces.Models;
using NodaTime;

namespace Energinet.DataHub.Reports.Application.MeasurementsReport.Handlers;

public sealed class RequestMeasurementsReportHandler : IRequestMeasurementsReportHandler
{
    private readonly IMeasurementsReportDatabricksJobsHelper _jobHelper;
    private readonly IMeasurementsReportRepository _repository;

    public RequestMeasurementsReportHandler(IMeasurementsReportDatabricksJobsHelper jobHelper, IMeasurementsReportRepository repository)
    {
        _jobHelper = jobHelper;
        _repository = repository;
    }

    public async Task<JobRunId> HandleAsync(RequestMeasurementsReportCommand request)
    {
        return await StartReportAsync(request).ConfigureAwait(false);
    }

    private async Task<JobRunId> StartReportAsync(RequestMeasurementsReportCommand request)
    {
        var reportRequestId = new ReportRequestId(Guid.NewGuid().ToString());

        var jobRunId = await _jobHelper.RunJobAsync(request.RequestDto, reportRequestId, request.ActorGln).ConfigureAwait(false);

        var measurementsReport = new MeasurementsReport(
            clock: SystemClock.Instance,
            userId: request.UserId,
            actorId: request.ActorId,
            jobRunId: jobRunId,
            reportRequestId: reportRequestId,
            request: request.RequestDto);

        await _repository.AddOrUpdateAsync(measurementsReport).ConfigureAwait(false);

        return jobRunId;
    }
}
