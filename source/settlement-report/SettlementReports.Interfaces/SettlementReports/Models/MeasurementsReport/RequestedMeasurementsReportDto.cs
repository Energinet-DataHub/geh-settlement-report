namespace Energinet.DataHub.SettlementReport.Interfaces.SettlementReports.Models.MeasurementsReport;

public sealed record RequestedMeasurementsReportDto(
    ReportRequestId RequestId,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    ReportStatus Status,
    Guid RequestedByActorId,
    IEnumerable<string> GridAreas,
    DateTimeOffset CreatedDateTime);
