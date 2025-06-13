using System.IO.Compression;
using Energinet.DataHub.Core.TestCommon.Xunit.Attributes;
using Energinet.DataHub.Core.TestCommon.Xunit.Orderers;
using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.MeasurementsReport;
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
            new DateTimeOffset(2022, 1, 12, 23, 0, 0, TimeSpan.Zero),
            null);

        _scenarioFixture.ScenarioState.MeasurementsReportRequestDto = new MeasurementsReportRequestDto(Filter: filter);
    }

    [SubsystemFact]
    [ScenarioStep(2)]
    public async Task When_ReportRequestIsSent()
    {
        // Act
        var jobRunId = await _scenarioFixture.MeasurementsReportClient.RequestAsync(
            _scenarioFixture.ScenarioState.MeasurementsReportRequestDto!,
            CancellationToken.None);

        // Assert
        Assert.NotNull(jobRunId);
        _scenarioFixture.ScenarioState.JobRunId = jobRunId;
    }

    [SubsystemFact]
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

    [SubsystemFact]
    [ScenarioStep(4)]
    public async Task AndThen_ReportCanBeDownloadedAndIsNotEmpty()
    {
        // Arrange
        var reportRequest = await _scenarioFixture.GetReportRequestByJobRunIdAsync(_scenarioFixture.ScenarioState.JobRunId!);
        Assert.NotNull(reportRequest);

        // Act
        var stream = await _scenarioFixture.MeasurementsReportClient.DownloadAsync(reportRequest.RequestId, CancellationToken.None);

        // Assert
        Assert.NotNull(stream);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        Assert.True(memoryStream.Length > 0, "The downloaded file is empty.");
    }

    [SubsystemFact]
    [ScenarioStep(5)]
    public async Task AndThen_ReportCanBeDownloadedAndContainsALeastOneRow()
    {
        // Arrange
        var reportRequest = await _scenarioFixture.GetReportRequestByJobRunIdAsync(_scenarioFixture.ScenarioState.JobRunId!);
        Assert.NotNull(reportRequest);

        // Act
        var stream = await _scenarioFixture.MeasurementsReportClient.DownloadAsync(reportRequest.RequestId, CancellationToken.None);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);

        memoryStream.Position = 0;

        using var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read);

        // Get the first CSV file from the ZIP
        var csvEntry = zipArchive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase));
        Assert.True(csvEntry != null, "No CSV file was found in the ZIP."); // Fails the test if no CSV file is found
        using var csvStream = csvEntry.Open();
        using var reader = new StreamReader(csvStream);

        var lines = new List<string>();
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (!string.IsNullOrWhiteSpace(line))
            {
                lines.Add(line);
            }
        }

        // Assert
        Assert.True(lines.Count > 1, "CSV does not contain any data rows."); // assuming first line is header
    }
}
