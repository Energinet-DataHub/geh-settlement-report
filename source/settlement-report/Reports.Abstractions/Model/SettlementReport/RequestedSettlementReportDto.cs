using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.SettlementReport;

namespace Energinet.DataHub.Reports.Interfaces.Models.SettlementReport;

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
