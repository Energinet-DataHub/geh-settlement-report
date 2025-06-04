namespace Energinet.DataHub.Reports.Common.Infrastructure.Security;

public enum FrontendActorMarketRole
{
    /// <summary>
    /// Other is used when a user's actor has a valid market role, but the role is currently irrelevant.
    /// </summary>
    Other,
    GridAccessProvider,
    EnergySupplier,
    SystemOperator,
    DataHubAdministrator,
}
