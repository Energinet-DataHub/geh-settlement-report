namespace Energinet.DataHub.Reports.Common.Infrastructure.Security;

public sealed record FrontendUser(Guid UserId, bool MultiTenancy, FrontendActor Actor);
