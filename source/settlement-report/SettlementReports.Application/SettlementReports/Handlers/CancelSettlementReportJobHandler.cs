using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Application.SettlementReports.Commands;
using Energinet.DataHub.Reports.Interfaces.Helpers;

namespace Energinet.DataHub.Reports.Application.SettlementReports.Handlers;

public sealed class CancelSettlementReportJobHandler : ICancelSettlementReportJobHandler
{
    private readonly ISettlementReportDatabricksJobsHelper _jobHelper;
    private readonly ISettlementReportRepository _repository;

    public CancelSettlementReportJobHandler(
        ISettlementReportDatabricksJobsHelper jobHelper,
        ISettlementReportRepository repository)
    {
        _jobHelper = jobHelper;
        _repository = repository;
    }

    public async Task HandleAsync(CancelSettlementReportCommand cancelSettlementReportCommand)
    {
        var report = await _repository
            .GetAsync(cancelSettlementReportCommand.RequestId.Id)
            .ConfigureAwait(false) ?? throw new InvalidOperationException("Report not found.");

        if (!report.JobId.HasValue)
        {
            throw new InvalidOperationException("Report does not have a JobId.");
        }

        if (report.UserId != cancelSettlementReportCommand.UserId)
        {
            throw new InvalidOperationException("UserId does not match. Only the user that started the report can cancel it.");
        }

        if (report.Status is not ReportStatus.InProgress)
        {
            throw new InvalidOperationException($"Can't cancel a report with status: {report.Status}");
        }

        await _jobHelper.CancelAsync(report.JobId.Value).ConfigureAwait(false);
    }
}
