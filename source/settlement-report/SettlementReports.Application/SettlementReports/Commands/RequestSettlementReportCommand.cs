using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.SettlementReport;

namespace Energinet.DataHub.Reports.Application.SettlementReports.Commands;

public sealed record RequestSettlementReportCommand(
    SettlementReportRequestDto RequestDto,
    Guid UserId,
    Guid ActorId,
    bool IsFas,
    string ActorGln,
    MarketRole MarketRole);
