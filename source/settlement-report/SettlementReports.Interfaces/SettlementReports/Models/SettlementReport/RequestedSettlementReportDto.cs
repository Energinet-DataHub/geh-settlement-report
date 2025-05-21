using Energinet.DataHub.SettlementReport.Interfaces.Models;

namespace Energinet.DataHub.SettlementReport.Interfaces.SettlementReports.Models.SettlementReport;

public sealed record RequestedSettlementReportDto(
    ReportRequestId RequestId,
    CalculationType CalculationType,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    ReportStatus Status,
    int GridAreaCount,
    double Progress,
    Guid RequestedByActorId,
    bool ContainsBasisData,
    bool SplitReportPerGridArea,
    bool IncludeMonthlyAmount,
    IReadOnlyDictionary<string, CalculationId?> GridAreas,
    JobRunId? JobId,
    DateTimeOffset CreatedDateTime,
    DateTimeOffset? EndedDateTime);
