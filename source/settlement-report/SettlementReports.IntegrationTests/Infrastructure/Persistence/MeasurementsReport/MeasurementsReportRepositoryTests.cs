using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.MeasurementsReport;
using Energinet.DataHub.Reports.Infrastructure.Persistence;
using Energinet.DataHub.Reports.Infrastructure.Persistence.MeasurementsReport;
using Energinet.DataHub.Reports.Test.Core.Fixture.Database;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Reports.IntegrationTests.Infrastructure.Persistence.MeasurementsReport;

public class MeasurementsReportRepositoryTests : IClassFixture<WholesaleDatabaseFixture<ReportsDatabaseContext>>
{
    private readonly WholesaleDatabaseManager<ReportsDatabaseContext> _databaseManager;

    public MeasurementsReportRepositoryTests(WholesaleDatabaseFixture<ReportsDatabaseContext> fixture)
    {
        _databaseManager = fixture.DatabaseManager;
    }

    [Fact]
    public async Task AddOrUpdateAsync_ValidRequest_PersistsChanges()
    {
        // arrange
        await using var context = _databaseManager.CreateDbContext();
        var target = new MeasurementsReportRepository(context);
        var requestFilterDto = new MeasurementsReportRequestFilterDto(
            new List<string> { "805", "806" },
            new DateTimeOffset(2024, 1, 1, 22, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 2, 1, 22, 0, 0, TimeSpan.Zero));

        var report = new Application.MeasurementsReport.MeasurementsReport(
            SystemClock.Instance,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new ReportRequestId(Guid.NewGuid().ToString()),
            new MeasurementsReportRequestDto(requestFilterDto));

        // act
        await target.AddOrUpdateAsync(report);

        // assert
        await using var readContext = _databaseManager.CreateDbContext();
        var actual = await readContext.MeasurementsReports.SingleOrDefaultAsync(x => x.Id == report.Id);

        Assert.NotNull(actual);
        Assert.Equal(report.Id, actual.Id);
        Assert.Equal(report.RequestId, actual.RequestId);
        Assert.Equal(report.UserId, actual.UserId);
        Assert.Equal(report.ActorId, actual.ActorId);
        Assert.Equal(report.CreatedDateTime, actual.CreatedDateTime);
        Assert.Equal(report.PeriodStart, actual.PeriodStart);
        Assert.Equal(report.PeriodEnd, actual.PeriodEnd);
        Assert.Equal(report.Status, actual.Status);
        Assert.Equal(report.BlobFileName, actual.BlobFileName);
        Assert.Equal(report.GridAreaCodes, actual.GridAreaCodes);
        Assert.Equal(report.JobRunId, actual.JobRunId);
        Assert.Equal(report.EndedDateTime, actual.EndedDateTime);
    }

    [Fact]
    public async Task GetByActorIdAsync_ActorIdMatches_ReturnsRequests()
    {
        // arrange
        await PrepareNewRequestAsync();
        await PrepareNewRequestAsync();

        var expectedRequest = await PrepareNewRequestAsync();

        await using var context = _databaseManager.CreateDbContext();
        var repository = new MeasurementsReportRepository(context);

        // act
        var actual = (await repository.GetByActorIdAsync(expectedRequest.ActorId)).ToList();

        // assert
        Assert.Single(actual);
        Assert.Equal(expectedRequest.Id, actual[0].Id);
    }

    [Fact]
    public async Task GetByRequestIdAsync_IdMatches_ReturnsRequests()
    {
        // arrange
        var expectedReportRequestId = new ReportRequestId(Guid.NewGuid().ToString());
        var notExpectedReportRequestId = new ReportRequestId(Guid.NewGuid().ToString());

        await PrepareNewRequestAsync(reportRequestId: expectedReportRequestId);
        await PrepareNewRequestAsync(reportRequestId: notExpectedReportRequestId);

        await using var context = _databaseManager.CreateDbContext();
        var repository = new MeasurementsReportRepository(context);

        // act
        var actual = await repository.GetByRequestIdAsync(expectedReportRequestId.Id);

        // assert
        Assert.Equal(actual.RequestId, expectedReportRequestId.Id);
    }

    private async Task<Application.MeasurementsReport.MeasurementsReport> PrepareNewRequestAsync(ReportRequestId? reportRequestId = null)
    {
        await using var setupContext = _databaseManager.CreateDbContext();
        var setupRepository = new MeasurementsReportRepository(setupContext);

        var requestFilterDto = new MeasurementsReportRequestFilterDto(
            ["805", "806"],
            new DateTimeOffset(2024, 1, 1, 22, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 2, 1, 22, 0, 0, TimeSpan.Zero));

        var measurementsReportRequest = new Application.MeasurementsReport.MeasurementsReport(
            SystemClock.Instance,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new JobRunId(Random.Shared.NextInt64()),
            reportRequestId ?? new ReportRequestId(Guid.NewGuid().ToString()),
            new MeasurementsReportRequestDto(requestFilterDto));

        await setupRepository.AddOrUpdateAsync(measurementsReportRequest);
        return measurementsReportRequest;
    }
}
