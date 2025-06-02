using Energinet.DataHub.Reports.Infrastructure.Persistence;
using Energinet.DataHub.Reports.Infrastructure.Persistence.MeasurementsReport;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.MeasurementsReport;
using Energinet.DataHub.Reports.Test.Core.Fixture.Database;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Reports.IntegrationTests.Infrastructure.SettlementReports_v2.Persistence.MeasurementsReport;

public class MeasurementsReportRepositoryTests : IClassFixture<WholesaleDatabaseFixture<SettlementReportDatabaseContext>>
{
    private readonly WholesaleDatabaseManager<SettlementReportDatabaseContext> _databaseManager;

    public MeasurementsReportRepositoryTests(WholesaleDatabaseFixture<SettlementReportDatabaseContext> fixture)
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

        var report = new Reports.Application.SettlementReports_v2.MeasurementsReport(
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
    public async Task GetAsync_ActorIdMatches_ReturnsRequests()
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

    private async Task<Reports.Application.SettlementReports_v2.MeasurementsReport> PrepareNewRequestAsync(
        Func<MeasurementsReportRequestFilterDto, Reports.Application.SettlementReports_v2.MeasurementsReport>? createReport = null)
    {
        await using var setupContext = _databaseManager.CreateDbContext();
        var setupRepository = new MeasurementsReportRepository(setupContext);

        var gridAreaCodes = new List<string> { "805", "806" };

        var requestFilterDto = new MeasurementsReportRequestFilterDto(
            gridAreaCodes,
            new DateTimeOffset(2024, 1, 1, 22, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 2, 1, 22, 0, 0, TimeSpan.Zero));

        var measurementsReportRequest = new Reports.Application.SettlementReports_v2.MeasurementsReport(
            SystemClock.Instance,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new ReportRequestId(Guid.NewGuid().ToString()),
            new MeasurementsReportRequestDto(requestFilterDto));

        if (createReport != null)
        {
            measurementsReportRequest = createReport(requestFilterDto);
        }

        await setupRepository.AddOrUpdateAsync(measurementsReportRequest);
        return measurementsReportRequest;
    }
}
