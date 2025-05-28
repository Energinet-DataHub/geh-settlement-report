using System.Collections.ObjectModel;
using System.Text.Json;
using Energinet.DataHub.Reports.Application.SettlementReports_v2;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.MeasurementsReport;

namespace Energinet.DataHub.Reports.Application.MeasurementsReport.Services;

public sealed class MeasurementsReportService : IMeasurementsReportService
{
    private readonly IMeasurementsReportRepository _measurementsReportRepository;

    public MeasurementsReportService(IMeasurementsReportRepository measurementsReportRepository)
    {
        _measurementsReportRepository = measurementsReportRepository;
    }

    public async Task<IEnumerable<RequestedMeasurementsReportDto>> GetReportsAsync(Guid actorId)
    {
        var settlementReports = (await _measurementsReportRepository
                .GetByActorIdAsync(actorId)
                .ConfigureAwait(false))
            .ToList();
        return settlementReports.Select(Map);
    }

    private static RequestedMeasurementsReportDto Map(SettlementReports_v2.MeasurementsReport report)
    {
        return new RequestedMeasurementsReportDto(
            new ReportRequestId(report.RequestId),
            report.PeriodStart.ToDateTimeOffset(),
            report.PeriodEnd.ToDateTimeOffset(),
            report.Status,
            report.ActorId,
            JsonSerializer.Deserialize<ReadOnlyCollection<string>>(report.GridAreaCodes) ?? ReadOnlyCollection<string>.Empty,
            report.CreatedDateTime.ToDateTimeOffset(),
            report.JobRunId is not null ? new JobRunId(report.JobRunId.Value) : null);
    }
}
