namespace Energinet.DataHub.Reports.Abstractions.Model.SettlementReport;

public sealed record SettlementReportRequestFilterDto(
    IReadOnlyDictionary<string, CalculationId?> GridAreas, // NOTE: Cannot type key to GridAreaCode, as serializer is unable to process the type.
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    CalculationType CalculationType,
    string? EnergySupplier,
    string? CsvFormatLocale);
