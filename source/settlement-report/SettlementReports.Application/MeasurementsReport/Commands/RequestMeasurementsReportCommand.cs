using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.MeasurementsReport;

namespace Energinet.DataHub.Reports.Application.MeasurementsReport.Commands;

public sealed record RequestMeasurementsReportCommand(
    MeasurementsReportRequestDto RequestDto,
    Guid UserId,
    Guid ActorId,
    bool IsFas,
    string ActorGln);
