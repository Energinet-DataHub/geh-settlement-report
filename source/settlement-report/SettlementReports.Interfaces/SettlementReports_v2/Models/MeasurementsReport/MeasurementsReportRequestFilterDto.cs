namespace Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.MeasurementsReport;

public sealed record MeasurementsReportRequestFilterDto(
    IEnumerable<string> GridAreaCodes,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd);
