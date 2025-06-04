using Energinet.DataHub.Reports.SubsystemTests.Fixtures;
using Energinet.DataHub.Reports.SubsystemTests.Fixtures.Identity;

namespace Energinet.DataHub.Reports.SubsystemTests.Features.SettlementReport.Fixtures;

public static class SettlementReportClientFactory
{
    public static async Task<ISettlementReportClient> CreateAsync(ReportsSubsystemTestConfiguration configuration)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(configuration.WebApiBaseAddress),
        };

        using var userAuthenticationClient = new B2CUserTokenAuthenticationClient(configuration.UserTokenConfiguration);
        var accessToken = await userAuthenticationClient.AcquireAccessTokenAsync();

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        return new SettlementReportClient(
            httpClient);
    }
}
