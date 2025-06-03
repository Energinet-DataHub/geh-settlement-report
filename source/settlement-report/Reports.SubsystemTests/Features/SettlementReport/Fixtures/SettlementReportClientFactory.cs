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

using Energinet.DataHub.Reports.Client;
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
