using Energinet.DataHub.Reports.Common.Infrastructure.Security;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models;

namespace Energinet.DataHub.Reports.WebAPI.Controllers.Mappers;

public static class MarketRoleMapper
{
    public static MarketRole MapToMarketRole(FrontendActorMarketRole marketRole)
    {
        return marketRole switch
        {
            FrontendActorMarketRole.Other => MarketRole.Other,
            FrontendActorMarketRole.GridAccessProvider => MarketRole.GridAccessProvider,
            FrontendActorMarketRole.EnergySupplier => MarketRole.EnergySupplier,
            FrontendActorMarketRole.SystemOperator => MarketRole.SystemOperator,
            FrontendActorMarketRole.DataHubAdministrator => MarketRole.DataHubAdministrator,
            _ => throw new ArgumentOutOfRangeException(nameof(marketRole)),
        };
    }
}
