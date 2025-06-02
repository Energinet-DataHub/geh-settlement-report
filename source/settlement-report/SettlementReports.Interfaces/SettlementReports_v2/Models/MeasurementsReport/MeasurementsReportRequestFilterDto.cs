using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.SettlementReport;

namespace Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.MeasurementsReport;

public sealed record MeasurementsReportRequestFilterDto(
    IReadOnlyDictionary<string, CalculationId?> GridAreaCodes,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd);
