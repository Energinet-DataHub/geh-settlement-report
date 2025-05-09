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

using Energinet.DataHub.Core.TestCommon.Xunit.Attributes;
using Energinet.DataHub.Core.TestCommon.Xunit.Orderers;
using Energinet.DataHub.Reports.SubsystemTests.Fixtures;
using Energinet.DataHub.SettlementReport.Interfaces.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using NodaTime;
using NodaTime.Text;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Reports.SubsystemTests;

[TestCaseOrderer(
    ordererTypeName: TestCaseOrdererLocation.OrdererTypeName,
    ordererAssemblyName: TestCaseOrdererLocation.OrdererAssemblyName)]
public class DownloadSettlementReportScenario : IClassFixture<SettlementReportFixture>,
    IAsyncLifetime
{
    private readonly SettlementReportFixture _fixture;

    public DownloadSettlementReportScenario(
        SettlementReportFixture fixture,
        ITestOutputHelper testOutputHelper)
    {
        _fixture = fixture;
        _fixture.SetTestOutputHelper(testOutputHelper);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _fixture.SetTestOutputHelper(null);
        return Task.CompletedTask;
    }

    [Fact]
    [ScenarioStep(1)]
    public void Given_ValidSettlementReportRequestDto()
    {
        var filter = new SettlementReportRequestFilterDto(
            GridAreas: new Dictionary<string, CalculationId?>
            {
                { "102", null },
            },
            PeriodStart: Instant.FromUtc(2022, 1, 1, 23, 0, 0).ToDateTimeOffset(),
            PeriodEnd: Instant.FromUtc(2022, 1, 3, 23, 0, 0).ToDateTimeOffset(),
            CalculationType: CalculationType.BalanceFixing,
            EnergySupplier: null,
            CsvFormatLocale: null);

        _fixture.SettlementReportRequestDto = new SettlementReportRequestDto(
            SplitReportPerGridArea: true,
            PreventLargeTextFiles: true,
            IncludeBasisData: true,
            IncludeMonthlyAmount: false,
            Filter: filter);
    }

    [Fact]
    [ScenarioStep(2)]
    public async Task AndGiven_SettlementReportRequestIsSent()
    {
        await _fixture.SettlementReportClient.RequestAsync(
            _fixture.SettlementReportRequestDto,
            CancellationToken.None);
    }
}
