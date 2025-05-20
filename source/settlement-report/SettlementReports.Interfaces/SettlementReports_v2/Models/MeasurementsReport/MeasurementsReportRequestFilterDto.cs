namespace Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.MeasurementsReport;

public sealed record MeasurementsReportRequestFilterDto(
    IEnumerable<string> GridAreas,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd);
