namespace Energinet.DataHub.Reports.Abstractions.Model.MeasurementsReport;

public sealed record MeasurementsReportRequestFilterDto(
    IReadOnlyCollection<string> GridAreaCodes,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd);
