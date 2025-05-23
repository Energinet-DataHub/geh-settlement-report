using Energinet.DataHub.Core.TestCommon.Xunit.Attributes;
using Energinet.DataHub.Core.TestCommon.Xunit.Orderers;
using Energinet.DataHub.Reports.SubsystemTests.Features.MeasurementsReport.Fixtures;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports.Models.MeasurementsReport;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Reports.SubsystemTests.Features.MeasurementsReport;

[TestCaseOrderer(
    ordererTypeName: TestCaseOrdererLocation.OrdererTypeName,
    ordererAssemblyName: TestCaseOrdererLocation.OrdererAssemblyName)]
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
            GridAreas: ["543"],
            PeriodStart: new DateTimeOffset(2022, 1, 11, 23, 0, 0, TimeSpan.Zero),
            PeriodEnd: new DateTimeOffset(2022, 1, 12, 23, 0, 0, TimeSpan.Zero));

        _scenarioFixture.ScenarioState.MeasurementsReportRequestDto = new MeasurementsReportRequestDto(
            Filter: filter,
            ActorNumberOverride: null,
            MarketRoleOverride: null);
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
}
