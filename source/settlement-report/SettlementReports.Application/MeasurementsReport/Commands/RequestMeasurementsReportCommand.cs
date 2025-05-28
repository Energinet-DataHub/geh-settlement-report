using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.MeasurementsReport;

namespace Energinet.DataHub.SettlementReport.Application.MeasurementsReport.Commands;

public sealed record RequestMeasurementsReportCommand(
    MeasurementsReportRequestDto RequestDto,
    string ActorGln);
