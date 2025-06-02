using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.SettlementReport;

namespace Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.MeasurementsReport;

public sealed record MeasurementsReportRequestFilterDto(
    IReadOnlyDictionary<string, CalculationId?> GridAreaCodes,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd);
