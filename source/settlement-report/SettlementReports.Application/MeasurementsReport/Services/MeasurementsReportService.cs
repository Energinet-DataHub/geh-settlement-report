using System.Text.Json;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.MeasurementsReport;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.SettlementReport;

namespace Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Services;

public sealed class MeasurementsReportService : IMeasurementsReportService
{
    private readonly IReportRepository<SettlementReports_v2.MeasurementsReport> _reportRepository;

    public MeasurementsReportService(IReportRepository<SettlementReports_v2.MeasurementsReport> reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<IEnumerable<RequestedMeasurementsReportDto>> GetReportsAsync(Guid actorId)
    {
        var reports = (await _reportRepository
                .GetByActorIdAsync(actorId)
                .ConfigureAwait(false))
            .ToList();
        return reports.Select(Map);
    }

    private static RequestedMeasurementsReportDto Map(SettlementReports_v2.MeasurementsReport report)
    {
        var gridAreas = string.IsNullOrEmpty(report.GridAreaCodes)
            ? []
            : JsonSerializer.Deserialize<Dictionary<string, CalculationId?>>(report.GridAreaCodes) ??
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
