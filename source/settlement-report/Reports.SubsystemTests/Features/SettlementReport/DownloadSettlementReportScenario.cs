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
using Energinet.DataHub.Reports.SubsystemTests.Features.SettlementReport.Fixtures;
using Energinet.DataHub.SettlementReport.Interfaces.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using FluentAssertions.Execution;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Reports.SubsystemTests.Features.SettlementReport;

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

    [SubsystemFact]
    [ScenarioStep(1)]
    public void Given_ValidSettlementReportRequestDto()
    {
        var filter = new SettlementReportRequestFilterDto(
            GridAreas: new Dictionary<string, CalculationId?>
            {
                { "543", null },
            },
            PeriodStart: new DateTimeOffset(2022, 1, 11, 23, 0, 0, TimeSpan.Zero),
            PeriodEnd: new DateTimeOffset(2022, 1, 12, 23, 0, 0, TimeSpan.Zero),
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

    [SubsystemFact]
    [ScenarioStep(2)]
    public async Task When_SettlementReportRequestIsSent()
    {
        var jobRunId = await _fixture.SettlementReportClient.RequestAsync(
            _fixture.SettlementReportRequestDto,
            CancellationToken.None);

        // Assert
        Assert.NotNull(jobRunId);
        _fixture.JobRunId = jobRunId;
    }

    [SubsystemFact]
    [ScenarioStep(3)]
    public async Task Then_ReportGenerationIsCompletedWithinWaitTime()
    {
        var (isCompletedOrFailed, reportRequest) = await _fixture.WaitForReportGenerationCompletedOrFailedAsync(
            _fixture.JobRunId!,
            TimeSpan.FromMinutes(15));

        // Assert
        using var assertionScope = new AssertionScope();
        Assert.True(isCompletedOrFailed);
        Assert.NotNull(reportRequest);
        Assert.Equal(SettlementReportStatus.Completed, reportRequest.Status);
    }
}
