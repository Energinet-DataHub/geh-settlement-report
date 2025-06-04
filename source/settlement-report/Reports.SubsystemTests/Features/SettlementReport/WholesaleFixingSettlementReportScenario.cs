using Energinet.DataHub.Core.TestCommon.Xunit.Attributes;
using Energinet.DataHub.Core.TestCommon.Xunit.Orderers;
using Energinet.DataHub.Reports.Abstractions.Model.SettlementReport;
using Energinet.DataHub.Reports.Interfaces.Models;
using Energinet.DataHub.Reports.Interfaces.Models.SettlementReport;
using Energinet.DataHub.Reports.SubsystemTests.Features.SettlementReport.Fixtures;
using FluentAssertions.Execution;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Reports.SubsystemTests.Features.SettlementReport;

[TestCaseOrderer(
    ordererTypeName: TestCaseOrdererLocation.OrdererTypeName,
    ordererAssemblyName: TestCaseOrdererLocation.OrdererAssemblyName)]
public class WholesaleFixingSettlementReportScenario : IClassFixture<SettlementReportScenarioFixture>,
    IAsyncLifetime
{
    private readonly SettlementReportScenarioFixture _scenarioFixture;

    public WholesaleFixingSettlementReportScenario(
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
    public void Given_ValidSettlementReportRequestDto()
    {
        // NOTE: These parameters match an existing calculation from the delta tables to ensure we get data (if no data, the Databricks job fails)
        var filter = new SettlementReportRequestFilterDto(
            GridAreas: new Dictionary<string, CalculationId?>
            {
                { "804", _scenarioFixture.Configuration.ExistingWholesaleFixingCalculationId },
            },
            PeriodStart: new DateTimeOffset(2023, 1, 31, 23, 0, 0, TimeSpan.Zero),
            PeriodEnd: new DateTimeOffset(2023, 2, 28, 23, 0, 0, TimeSpan.Zero),
            CalculationType: CalculationType.WholesaleFixing,
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
            TimeSpan.FromMinutes(20));

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
        var stream = await _scenarioFixture.SettlementReportClient.DownloadAsync(reportRequest.RequestId!, CancellationToken.None);

        // Assert
        Assert.NotNull(stream);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        Assert.True(memoryStream.Length > 0, "The downloaded file is empty.");
    }
}
