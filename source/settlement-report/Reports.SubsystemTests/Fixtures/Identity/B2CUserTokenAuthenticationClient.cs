using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Energinet.DataHub.Reports.SubsystemTests.Fixtures.Identity;

/// <summary>
/// Encapsulates REST call to ROPC flow for retrieving a user access token.
///
/// See also "Test the ROPC flow": https://learn.microsoft.com/en-us/azure/active-directory-b2c/add-ropc-policy?#test-the-ropc-flow
/// </summary>
public sealed class B2CUserTokenAuthenticationClient : IDisposable
{
    /// <summary>
    /// Class to easily parse access token from JSON.
    /// </summary>
    private class AccessTokenResult
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
            = string.Empty;
    }

    public B2CUserTokenAuthenticationClient(B2CUserTokenConfiguration configuration)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        UserTokenPermissionAugmentation = new UserTokenPermissionAugmentation(configuration);
        AuthenticationHttpClient = new HttpClient();
    }

    private UserTokenPermissionAugmentation UserTokenPermissionAugmentation { get; }

    private B2CUserTokenConfiguration Configuration { get; }

    private HttpClient AuthenticationHttpClient { get; }

    public void Dispose()
    {
        UserTokenPermissionAugmentation.Dispose();
        AuthenticationHttpClient.Dispose();
    }

    /// <summary>
    /// Acquire an access token for the configured user.
    /// </summary>
    /// <returns>Access token.</returns>
    public async Task<string> AcquireAccessTokenAsync()
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(Configuration.Username), "username" },
            { new StringContent(Configuration.Password), "password" },
            { new StringContent("password"), "grant_type" },
            { new StringContent($"openid {Configuration.BackendBffScope} offline_access"), "scope" },
            { new StringContent(Configuration.FrontendAppId), "client_id" },
            { new StringContent("token id_token"), "response_type" },
        };

        var httpResponse = await AuthenticationHttpClient.PostAsync(Configuration.RopcUrl, form);
        httpResponse.EnsureSuccessStatusCode();

        var tokenResult = await httpResponse.Content.ReadFromJsonAsync<AccessTokenResult>();
        var externalAccessToken = tokenResult!.AccessToken;

        return await UserTokenPermissionAugmentation.AugmentAccessTokenWithPermissionsAsync(externalAccessToken);
    }
}
