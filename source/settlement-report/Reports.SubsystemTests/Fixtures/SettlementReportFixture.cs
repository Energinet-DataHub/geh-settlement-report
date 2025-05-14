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

using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Energinet.DataHub.Core.TestCommon.Diagnostics;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Reports.SubsystemTests.Fixtures;

public class SettlementReportFixture : IAsyncLifetime
{
    public SettlementReportFixture()
    {
        Logger = new TestDiagnosticsLogger();

        Configuration = new SettlementReportSubsystemTestConfiguration();
    }

    [NotNull]
    public SettlementReportRequestDto? SettlementReportRequestDto { get; set; }

    /// <summary>
    /// The actual client is not created until <see cref="InitializeAsync"/> has been called by the base class.
    /// </summary>
    public SettlementReportClient SettlementReportClient { get; private set; } = null!;

    private SettlementReportSubsystemTestConfiguration Configuration { get; }

    private TestDiagnosticsLogger Logger { get; }

    public async Task InitializeAsync()
    {
        SettlementReportClient = await WebApiClientFactory.CreateSettlementReportClientAsync(Configuration);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public void SetTestOutputHelper(ITestOutputHelper? testOutputHelper)
    {
        Logger.TestOutputHelper = testOutputHelper;
    }
}
