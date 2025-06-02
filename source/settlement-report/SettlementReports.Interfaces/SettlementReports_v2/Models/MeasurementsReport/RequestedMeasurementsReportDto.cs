using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.SettlementReport;

namespace Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.MeasurementsReport;

public sealed record RequestedMeasurementsReportDto(
    ReportRequestId RequestId,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    ReportStatus Status,
    Guid RequestedByActorId,
    IReadOnlyDictionary<string, CalculationId?> GridAreaCodes,
    DateTimeOffset CreatedDateTime,
    JobRunId? JobRunId);
