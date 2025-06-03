using System.Security.Claims;
using Energinet.DataHub.Core.App.Common.Abstractions.Users;

namespace Energinet.DataHub.Reports.Common.Infrastructure.Security;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class FrontendUserProvider : IUserProvider<FrontendUser>
{
    private const string ActorNumberClaim = "actornumber";
    private const string MarketRolesClaim = "marketroles";
    private const string GridAreasClaim = "gridareas";

    public Task<FrontendUser?> ProvideUserAsync(
        Guid userId,
        Guid actorId,
        bool multiTenancy,
        IEnumerable<Claim> claims)
    {
        var enumeratedClaims = claims.ToList();
        var frontendActor = new FrontendActor(actorId, GetActorNumber(enumeratedClaims), GetMarketRole(enumeratedClaims), GetGridAreas(enumeratedClaims));
        var frontendUser = new FrontendUser(userId, multiTenancy, frontendActor);

        return Task.FromResult<FrontendUser?>(frontendUser);
    }

    private static string GetActorNumber(IEnumerable<Claim> claims)
    {
        return claims.Single(claim => claim.Type == ActorNumberClaim).Value;
    }

    private static FrontendActorMarketRole GetMarketRole(IEnumerable<Claim> claims)
    {
        return claims.Single(claim => claim.Type == MarketRolesClaim).Value switch
        {
            "GridAccessProvider" => FrontendActorMarketRole.GridAccessProvider,
            "EnergySupplier" => FrontendActorMarketRole.EnergySupplier,
            "SystemOperator" => FrontendActorMarketRole.SystemOperator,
            "DataHubAdministrator" => FrontendActorMarketRole.DataHubAdministrator,
            _ => FrontendActorMarketRole.Other,
        };
    }

    private static IEnumerable<string> GetGridAreas(IEnumerable<Claim> claims)
    {
        return
            claims.SingleOrDefault(claim => claim.Type == GridAreasClaim)?.Value.Split(",", StringSplitOptions.RemoveEmptyEntries)
            ?? Enumerable.Empty<string>();
    }
}
