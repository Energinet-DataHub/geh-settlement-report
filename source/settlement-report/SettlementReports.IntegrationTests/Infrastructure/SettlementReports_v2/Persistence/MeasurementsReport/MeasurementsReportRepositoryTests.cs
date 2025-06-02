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

using Energinet.DataHub.Reports.Infrastructure.Persistence;
using Energinet.DataHub.Reports.Infrastructure.Persistence.MeasurementsReport;
using Energinet.DataHub.Reports.Interfaces.Models;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.MeasurementsReport;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.SettlementReport;
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
            new Dictionary<string, CalculationId?> { { "805", null }, { "806", null } },
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

        var calculationFilter = new Dictionary<string, CalculationId?>
        {
            { "805", new CalculationId(Guid.Parse("D116DD8A-898E-48F1-8200-D31D12F82545")) },
            { "806", new CalculationId(Guid.Parse("D116DD8A-898E-48F1-8200-D31D12F82545")) },
        };

        var requestFilterDto = new MeasurementsReportRequestFilterDto(
            calculationFilter,
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
