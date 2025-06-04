using System.Text.Json;
using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.MeasurementsReport;
using Energinet.DataHub.Reports.Interfaces.Helpers;

namespace Energinet.DataHub.Reports.Application.MeasurementsReport.Services;

public sealed class MeasurementsReportService : IMeasurementsReportService
{
    private readonly IMeasurementsReportRepository _measurementsReportRepository;
    private readonly IMeasurementsReportDatabricksJobsHelper _jobHelper;

    public MeasurementsReportService(
        IMeasurementsReportRepository measurementsReportRepository,
        IMeasurementsReportDatabricksJobsHelper jobHelper)
    {
        _measurementsReportRepository = measurementsReportRepository;
        _jobHelper = jobHelper;
    }

    public async Task<IEnumerable<RequestedMeasurementsReportDto>> GetReportsAsync(Guid actorId)
    {
        var settlementReports = (await _measurementsReportRepository
                .GetByActorIdAsync(actorId)
                .ConfigureAwait(false))
            .ToList();
        return settlementReports.Select(Map);
    }

    public async Task CancelAsync(ReportRequestId reportRequestId, Guid userId)
    {
        var report = await _measurementsReportRepository.GetByRequestIdAsync(reportRequestId.Id)
            .ConfigureAwait(false) ?? throw new InvalidOperationException("Report not found.");

        if (!report.JobRunId.HasValue)
        {
            throw new InvalidOperationException("Report does not have a JobRunId.");
        }

        if (report.UserId != userId)
        {
            throw new InvalidOperationException("UserId does not match. Only the user that started the report can cancel it.");
        }

        if (report.Status is not ReportStatus.InProgress)
        {
            throw new InvalidOperationException($"Can't cancel a report with status: {report.Status}.");
        }

        await _jobHelper.CancelAsync(report.JobRunId.Value).ConfigureAwait(false);
    }

    private static RequestedMeasurementsReportDto Map(MeasurementsReport report)
    {
        var gridAreas = string.IsNullOrEmpty(report.GridAreaCodes)
            ? []
            : JsonSerializer.Deserialize<List<string>>(report.GridAreaCodes) ??
              [];

        return new RequestedMeasurementsReportDto(
            new ReportRequestId(report.RequestId),
            report.PeriodStart.ToDateTimeOffset(),
            report.PeriodEnd.ToDateTimeOffset(),
            report.Status,
            report.ActorId,
            gridAreas,
            report.CreatedDateTime.ToDateTimeOffset(),
            report.JobRunId is not null ? new JobRunId(report.JobRunId.Value) : null);
    }
}
