namespace Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.MeasurementsReport;

public sealed record RequestedMeasurementsReportDto(
    SettlementReportRequestId RequestId,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    SettlementReportStatus Status,
    Guid RequestedByActorId,
    IEnumerable<string> GridAreas,
    DateTimeOffset CreatedDateTime);
