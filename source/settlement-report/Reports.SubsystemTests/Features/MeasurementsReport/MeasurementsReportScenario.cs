using Energinet.DataHub.Core.TestCommon.Xunit.Attributes;
using Energinet.DataHub.Core.TestCommon.Xunit.Orderers;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.MeasurementsReport;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.SettlementReport;
using Energinet.DataHub.Reports.SubsystemTests.Features.MeasurementsReport.Fixtures;
using FluentAssertions.Execution;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Reports.SubsystemTests.Features.MeasurementsReport;

[TestCaseOrderer(
    TestCaseOrdererLocation.OrdererTypeName,
    TestCaseOrdererLocation.OrdererAssemblyName)]
public class MeasurementsReportScenario : IClassFixture<MeasurementsReportScenarioFixture>,
    IAsyncLifetime
{
    private readonly MeasurementsReportScenarioFixture _scenarioFixture;

    public MeasurementsReportScenario(
        MeasurementsReportScenarioFixture scenarioFixture,
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
    public void Given_ValidReportRequest()
    {
        var filter = new MeasurementsReportRequestFilterDto(
            new List<string> { "543" },
            new DateTimeOffset(2022, 1, 11, 23, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2022, 1, 12, 23, 0, 0, TimeSpan.Zero));

        _scenarioFixture.ScenarioState.MeasurementsReportRequestDto = new MeasurementsReportRequestDto(Filter: filter);
    }

    [SubsystemFact]
    [ScenarioStep(2)]
    public async Task When_ReportRequestIsSent()
    {
        // Act
        var jobRunId = await _scenarioFixture.ReportsClient.RequestAsync(
            _scenarioFixture.ScenarioState.MeasurementsReportRequestDto!,
            CancellationToken.None);

        // Assert
        Assert.NotNull(jobRunId);
        _scenarioFixture.ScenarioState.JobRunId = jobRunId;
    }

    [SubsystemFact(Skip = "Skipped for now due to missing implementations.")]
    [ScenarioStep(3)]
    public async Task Then_ReportGenerationIsCompletedWithinWaitTime()
    {
        var (isCompletedOrFailed, reportRequest) = await _scenarioFixture.WaitForReportGenerationCompletedOrFailedAsync(
            _scenarioFixture.ScenarioState.JobRunId!,
            TimeSpan.FromMinutes(15));

        // Assert
        using var assertionScope = new AssertionScope();
        Assert.True(isCompletedOrFailed);
        Assert.NotNull(reportRequest);
        Assert.Equal(ReportStatus.Completed, reportRequest.Status);
    }
}
