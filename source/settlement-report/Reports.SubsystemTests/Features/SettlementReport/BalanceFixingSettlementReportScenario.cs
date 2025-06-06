﻿using Energinet.DataHub.Core.TestCommon.Xunit.Attributes;
using Energinet.DataHub.Core.TestCommon.Xunit.Orderers;
using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.SettlementReport;
using Energinet.DataHub.Reports.SubsystemTests.Features.SettlementReport.Fixtures;
using FluentAssertions.Execution;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Reports.SubsystemTests.Features.SettlementReport;

[TestCaseOrderer(
    ordererTypeName: TestCaseOrdererLocation.OrdererTypeName,
    ordererAssemblyName: TestCaseOrdererLocation.OrdererAssemblyName)]
public class BalanceFixingSettlementReportScenario : IClassFixture<SettlementReportScenarioFixture>,
    IAsyncLifetime
{
    private readonly SettlementReportScenarioFixture _scenarioFixture;

    public BalanceFixingSettlementReportScenario(
        SettlementReportScenarioFixture scenarioFixture,
        ITestOutputHelper testOutputHelper)
    {
        _scenarioFixture = scenarioFixture;
        _scenarioFixture.SetTestOutputHelper(testOutputHelper);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _scenarioFixture.SetTestOutputHelper(null);
        return Task.CompletedTask;
    }

    [SubsystemFact]
    [ScenarioStep(1)]
    public void Given_ValidSettlementReportRequest()
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

        _scenarioFixture.SettlementReportScenarioState.SettlementReportRequestDto = new SettlementReportRequestDto(
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
        var jobRunId = await _scenarioFixture.SettlementReportClient.RequestAsync(
            _scenarioFixture.SettlementReportScenarioState.SettlementReportRequestDto!,
            CancellationToken.None);

        // Assert
        Assert.NotNull(jobRunId);
        _scenarioFixture.SettlementReportScenarioState.JobRunId = jobRunId;
    }

    [SubsystemFact]
    [ScenarioStep(3)]
    public async Task Then_ReportGenerationIsCompletedWithinWaitTime()
    {
        var (isCompletedOrFailed, reportRequest) = await _scenarioFixture.WaitForReportGenerationCompletedOrFailedAsync(
            _scenarioFixture.SettlementReportScenarioState.JobRunId!,
            TimeSpan.FromMinutes(15));

        // Assert
        using var assertionScope = new AssertionScope();
        Assert.True(isCompletedOrFailed);
        Assert.NotNull(reportRequest);
        Assert.Equal(ReportStatus.Completed, reportRequest.Status);
    }

    [SubsystemFact]
    [ScenarioStep(4)]
    public async Task AndThen_ReportCanBeDownloadedAndIsNotEmpty()
    {
        // Arrange
        var reportRequest = await _scenarioFixture.GetReportRequestByJobRunIdAsync(_scenarioFixture.SettlementReportScenarioState.JobRunId!);
        Assert.NotNull(reportRequest);

        // Act
        var stream = await _scenarioFixture.SettlementReportClient.DownloadAsync(reportRequest.RequestId, CancellationToken.None);

        // Assert
        Assert.NotNull(stream);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        Assert.True(memoryStream.Length > 0, "The downloaded file is empty.");
    }
}
