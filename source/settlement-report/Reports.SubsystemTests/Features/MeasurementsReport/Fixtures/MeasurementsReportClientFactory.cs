using Energinet.DataHub.Reports.Client;
using Energinet.DataHub.Reports.SubsystemTests.Fixtures;
using Energinet.DataHub.Reports.SubsystemTests.Fixtures.Identity;

namespace Energinet.DataHub.Reports.SubsystemTests.Features.MeasurementsReport.Fixtures;

public static class MeasurementsReportClientFactory
{
    public static async Task<IMeasurementsReportClient> CreateAsync(ReportsSubsystemTestConfiguration configuration)
    {
        var httpClient = new HttpClient { BaseAddress = new Uri(configuration.WebApiBaseAddress) };

        await AddAuthorizationToken(configuration, httpClient);

        return new MeasurementsReportClient(
            httpClient);
    }

    public static IMeasurementsReportClient CreateUnauthorized(ReportsSubsystemTestConfiguration configuration)
    {
        var httpClient = new HttpClient { BaseAddress = new Uri(configuration.WebApiBaseAddress) };

        return new MeasurementsReportClient(
            httpClient);
    }

    private static async Task AddAuthorizationToken(
        ReportsSubsystemTestConfiguration configuration,
        HttpClient httpClient)
    {
        using var userAuthenticationClient = new B2CUserTokenAuthenticationClient(configuration.UserTokenConfiguration);
        var accessToken = await userAuthenticationClient.AcquireAccessTokenAsync();

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
    }
}
