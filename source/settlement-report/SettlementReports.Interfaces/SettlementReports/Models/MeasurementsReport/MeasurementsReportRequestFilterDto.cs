namespace Energinet.DataHub.SettlementReport.Interfaces.SettlementReports.Models.MeasurementsReport;

public sealed record MeasurementsReportRequestFilterDto(
    IEnumerable<string> GridAreas,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd);
