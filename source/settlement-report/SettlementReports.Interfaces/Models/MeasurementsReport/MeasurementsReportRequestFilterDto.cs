namespace Energinet.DataHub.Reports.Interfaces.Models.MeasurementsReport;

public sealed record MeasurementsReportRequestFilterDto(
    IReadOnlyCollection<string> GridAreaCodes,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd);
