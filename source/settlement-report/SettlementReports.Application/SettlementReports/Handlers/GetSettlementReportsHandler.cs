using System.Text.Json;
using Energinet.DataHub.Reports.Application.Services;
using Energinet.DataHub.Reports.Interfaces;
using Energinet.DataHub.Reports.Interfaces.Models;
using Energinet.DataHub.Reports.Interfaces.Models.SettlementReport;

namespace Energinet.DataHub.Reports.Application.SettlementReports.Handlers;

public sealed class GetSettlementReportsHandler : IGetSettlementReportsHandler
{
    private readonly ISettlementReportRepository _settlementReportRepository;
    private readonly IRemoveExpiredSettlementReports _removeExpiredSettlementReports;

    public GetSettlementReportsHandler(
        ISettlementReportRepository settlementReportRepository,
        IRemoveExpiredSettlementReports removeExpiredSettlementReports)
    {
        _settlementReportRepository = settlementReportRepository;
        _removeExpiredSettlementReports = removeExpiredSettlementReports;
    }

    public async Task<IEnumerable<RequestedSettlementReportDto>> GetForJobsAsync()
    {
        var settlementReports = (await _settlementReportRepository
                .GetForJobsAsync()
                .ConfigureAwait(false))
            .ToList();

        await _removeExpiredSettlementReports.RemoveExpiredAsync(settlementReports).ConfigureAwait(false);
        return settlementReports.Select(Map);
    }

    public async Task<IEnumerable<RequestedSettlementReportDto>> GetForJobsAsync(Guid actorId)
    {
        var settlementReports = (await _settlementReportRepository
                .GetForJobsAsync(actorId)
                .ConfigureAwait(false))
            .ToList();

        await _removeExpiredSettlementReports.RemoveExpiredAsync(settlementReports).ConfigureAwait(false);
        return settlementReports.Select(Map);
    }

    private static RequestedSettlementReportDto Map(SettlementReport report)
    {
        var gridAreas = string.IsNullOrEmpty(report.GridAreas)
            ? new Dictionary<string, CalculationId?>()
            : JsonSerializer.Deserialize<Dictionary<string, CalculationId?>>(report.GridAreas) ??
              new Dictionary<string, CalculationId?>();

        return new RequestedSettlementReportDto(
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
