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

using WireMock.Server;

namespace Energinet.DataHub.SettlementReport.Orchestration.SettlementReports.IntegrationTests.Extensions;

/// <summary>
/// A collection of extensions methods that provides abstractions on top of
/// the more technical databricks api extensions in <see cref="DatabricksApiWireMockExtensions"/>
/// </summary>
public static class DatabricksAbstractionExtensions
{
    public static WireMockServer MockEnergyResultsViewResponse(
        this WireMockServer server,
        Guid? calculationId = null)
    {
        // => Databricks SQL Statement API
        var chunkIndex = 0;
        var statementId = Guid.NewGuid().ToString();
        var path = "GetDatabricksDataPath";

        server
            .MockEnergySqlStatementsView(statementId, chunkIndex)
            .MockEnergySqlStatementsResultChunks(statementId, chunkIndex, path)
            .MockEnergySqlStatementsResultViewStream(path, calculationId ?? Guid.NewGuid());

        return server;
    }
}
