using Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Commands;
using Energinet.DataHub.SettlementReport.Interfaces.Helpers;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Handlers;

public sealed class RequestMeasurementsReportJobHandler : IRequestMeasurementsReportJobHandler
{
    private readonly IMeasurementsReportDatabricksJobsHelper _jobHelper;

    public RequestMeasurementsReportJobHandler(IMeasurementsReportDatabricksJobsHelper jobHelper)
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

        var runId = await _jobHelper.RunJobAsync(request.RequestDto, reportId).ConfigureAwait(false);

        // Eventually the report will be added to the database here
        return runId;
    }
}
