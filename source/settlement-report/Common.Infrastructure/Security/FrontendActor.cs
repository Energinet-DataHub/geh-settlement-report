namespace Energinet.DataHub.Reports.Common.Infrastructure.Security;

public sealed record FrontendActor(
    Guid ActorId,
    string ActorNumber,
    FrontendActorMarketRole MarketRole,
    IEnumerable<string> GridAreas);
