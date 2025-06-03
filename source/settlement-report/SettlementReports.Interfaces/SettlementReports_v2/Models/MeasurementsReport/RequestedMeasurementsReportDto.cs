namespace Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.MeasurementsReport;

public sealed record RequestedMeasurementsReportDto(
    ReportRequestId RequestId,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    ReportStatus Status,
    Guid RequestedByActorId,
    IReadOnlyCollection<string> GridAreaCodes,
    DateTimeOffset CreatedDateTime,
    JobRunId? JobRunId);
