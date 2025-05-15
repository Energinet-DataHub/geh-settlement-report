// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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

        var internalKeyVaultName = Root.GetValue<string>("INTERNAL_KEYVAULT_NAME")
                                ?? throw new NullReferenceException($"Missing configuration value for INTERNAL_KEYVAULT_NAME");

        var keyVaultConfiguration = GetKeyVaultConfiguration(sharedKeyVaultName, internalKeyVaultName);

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
    private IConfigurationRoot GetKeyVaultConfiguration(string sharedKeyVaultName, string internalKeyVaultName)
    {
        var sharedKeyVaultUrl = $"https://{sharedKeyVaultName}.vault.azure.net/";
        var internalKeyVaultUrl = $"https://{internalKeyVaultName}.vault.azure.net/";

        return new ConfigurationBuilder()
            .AddAuthenticatedAzureKeyVault(sharedKeyVaultUrl)
            .AddAuthenticatedAzureKeyVault(internalKeyVaultUrl)
            .Build();
    }
}
