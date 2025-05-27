using System.Text.Json;
using Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.MeasurementsReport;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.SettlementReport;

namespace Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Services;

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
        var gridAreas = string.IsNullOrEmpty(report.GridAreas)
            ? new Dictionary<string, CalculationId?>()
            : JsonSerializer.Deserialize<Dictionary<string, CalculationId?>>(report.GridAreas) ??
              new Dictionary<string, CalculationId?>();

        return new RequestedMeasurementsReportDto(
            new ReportRequestId(report.RequestId),
            report.CalculationType,
            report.PeriodStart.ToDateTimeOffset(),
            report.PeriodEnd.ToDateTimeOffset(),
            report.Status,
            report.GridAreaCount,
            0,
            report.ActorId,
            report.ContainsBasisData,
            report.SplitReportPerGridArea,
            report.IncludeMonthlyAmount,
            gridAreas,
            report.JobId is not null ? new JobRunId(report.JobId.Value) : null,
            report.CreatedDateTime.ToDateTimeOffset(),
            report.EndedDateTime?.ToDateTimeOffset());
    }
}
