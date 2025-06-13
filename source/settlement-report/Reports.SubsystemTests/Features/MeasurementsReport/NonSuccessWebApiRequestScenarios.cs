using System.Net;
using Energinet.DataHub.Core.TestCommon.Xunit.Attributes;
using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.MeasurementsReport;
using Energinet.DataHub.Reports.SubsystemTests.Features.MeasurementsReport.Fixtures;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Energinet.DataHub.Reports.SubsystemTests.Features.MeasurementsReport;

public class NonSuccessWebApiRequestScenarios : IClassFixture<MeasurementsReportScenarioFixture>,
    IAsyncLifetime
{
    private readonly MeasurementsReportScenarioFixture _scenarioFixture;

    public NonSuccessWebApiRequestScenarios(
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
    public async Task GivenReportRequest_WhenUnauthorizedRequest_ThenResponseIsUnauthorized()
    {
        // Arrange
        var anyFilter = CreateBadFilter();
        var anyRequest = new MeasurementsReportRequestDto(anyFilter);

        // Act
        try
        {
            await _scenarioFixture.UnauthorizedMeasurementsReportClient.RequestAsync(anyRequest, CancellationToken.None);
        }
        catch (HttpRequestException ex)
        {
            ex.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    [SubsystemFact]
    public async Task GivenListRequest_WhenUnauthorizedRequest_ThenResponseIsUnauthorized()
    {
        try
        {
            // Act
            await _scenarioFixture.UnauthorizedMeasurementsReportClient.GetAsync(CancellationToken.None);
        }
        catch (HttpRequestException ex)
        {
            // Assert
            ex.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    [SubsystemFact]
    public async Task GivenDownloadRequest_WhenUnauthorizedRequest_ThenResponseIsUnauthorized()
    {
        // Arrange
        var anyReportRequestId = new ReportRequestId(Guid.NewGuid().ToString());

        try
        {
            // Act
            await _scenarioFixture.UnauthorizedMeasurementsReportClient.DownloadAsync(anyReportRequestId, CancellationToken.None);
        }
        catch (HttpRequestException ex)
        {
            // Assert
            ex.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }

    [SubsystemFact]
    public async Task GivenDownloadRequest_WhenBadRequest_ThenResponseIsUnauthorized()
    {
        // Arrange
        var nonExistingReportId = new ReportRequestId(Guid.NewGuid().ToString());

        try
        {
            // Act
            await _scenarioFixture.MeasurementsReportClient.DownloadAsync(
                nonExistingReportId,
                CancellationToken.None);
        }
        catch (HttpRequestException ex)
        {
            // Assert
            ex.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }

    private MeasurementsReportRequestFilterDto CreateBadFilter()
    {
        return new MeasurementsReportRequestFilterDto(
            GridAreaCodes: new List<string> { "543" },
            PeriodStart: new DateTimeOffset(2022, 1, 11, 23, 0, 0, TimeSpan.Zero),
            PeriodEnd: new DateTimeOffset(2022, 1, 12, 23, 0, 0, TimeSpan.Zero),
            EnergySupplier: null);
    }
}
