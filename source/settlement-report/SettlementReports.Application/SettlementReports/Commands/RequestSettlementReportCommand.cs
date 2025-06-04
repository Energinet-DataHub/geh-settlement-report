using Energinet.DataHub.Reports.Interfaces.Models;
using Energinet.DataHub.Reports.Interfaces.Models.SettlementReport;

namespace Energinet.DataHub.Reports.Application.SettlementReports.Commands;

public sealed record RequestSettlementReportCommand(
    SettlementReportRequestDto RequestDto,
    Guid UserId,
    Guid ActorId,
    bool IsFas,
    string ActorGln,
    MarketRole MarketRole);
