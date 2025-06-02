using Energinet.DataHub.Reports.Application.MeasurementsReport.Commands;
using Energinet.DataHub.Reports.Interfaces.Helpers;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.Reports.Application.MeasurementsReport.Handlers;

public sealed class RequestMeasurementsReportHandler : IRequestMeasurementsReportHandler
{
    private readonly IMeasurementsReportDatabricksJobsHelper _jobHelper;
    private readonly IMeasurementsReportPersistenceService _measurementsReportPersistenceService;

    public RequestMeasurementsReportHandler(IMeasurementsReportDatabricksJobsHelper jobHelper)
    {
        _jobHelper = jobHelper;
    }

    public async Task<JobRunId> HandleAsync(RequestMeasurementsReportCommand request)
    {
        return await StartReportAsync(request).ConfigureAwait(false);
    }

    private async Task<JobRunId> StartReportAsync(RequestMeasurementsReportCommand request)
    {
        var reportId = new ReportRequestId(Guid.NewGuid().ToString());

        var runId = await _jobHelper.RunJobAsync(request.RequestDto, reportId, request.ActorGln).ConfigureAwait(false);

        await _measurementsReportPersistenceService
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
