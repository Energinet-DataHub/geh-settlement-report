using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.TestCommon.Xunit.Configuration;
using Energinet.DataHub.Reports.SubsystemTests.Fixtures.Identity;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Reports.SubsystemTests.Fixtures;

public class ReportsSubsystemTestConfiguration : SubsystemTestConfiguration
{
    public ReportsSubsystemTestConfiguration()
    {
        var sharedKeyVaultName = Root.GetValue<string>("SHARED_KEYVAULT_NAME")
                                ?? throw new NullReferenceException($"Missing configuration value for SHARED_KEYVAULT_NAME");

        var keyVaultConfiguration = GetKeyVaultConfiguration(sharedKeyVaultName);

        WebApiBaseAddress = keyVaultConfiguration.GetValue<string>("app-settlement-report-webapi-base-url")
            ?? throw new ArgumentNullException(nameof(WebApiBaseAddress), $"Missing configuration value for {nameof(WebApiBaseAddress)}");

        UserTokenConfiguration = B2CUserTokenConfiguration.CreateFromConfiguration(Root);
    }

    public string WebApiBaseAddress { get; }

    /// <summary>
    /// Settings necessary to retrieve a user token for authentication with Wholesale Web API in live environment.
    /// </summary>
    public B2CUserTokenConfiguration UserTokenConfiguration { get; }

    /// <summary>
    /// Build configuration for loading settings from key vault secrets.
    /// </summary>
    private IConfigurationRoot GetKeyVaultConfiguration(string sharedKeyVaultName)
    {
        var sharedKeyVaultUrl = $"https://{sharedKeyVaultName}.vault.azure.net/";

        return new ConfigurationBuilder()
            .AddAuthenticatedAzureKeyVault(sharedKeyVaultUrl)
            .Build();
    }
}
