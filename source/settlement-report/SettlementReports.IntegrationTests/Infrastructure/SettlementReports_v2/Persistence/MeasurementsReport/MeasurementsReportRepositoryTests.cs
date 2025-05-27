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

using Energinet.DataHub.SettlementReport.Infrastructure.Persistence;
using Energinet.DataHub.SettlementReport.Infrastructure.Persistence.MeasurementsReport;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.MeasurementsReport;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.SettlementReport;
using Energinet.DataHub.SettlementReport.Test.Core.Fixture.Database;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.SettlementReports.IntegrationTests.Infrastructure.SettlementReports_v2.Persistence.MeasurementsReport;

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

        var calculationFilter = new Dictionary<string, CalculationId?>
        {
            { "805", new CalculationId(Guid.Parse("D116DD8A-898E-48F1-8200-D31D12F82545")) }, { "806", new CalculationId(Guid.Parse("D116DD8A-898E-48F1-8200-D31D12F82545")) },
        };

        var requestFilterDto = new MeasurementsReportRequestFilterDto(
            ["805", "806"],
            new DateTimeOffset(2024, 1, 1, 22, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 2, 1, 22, 0, 0, TimeSpan.Zero));

        var report = new SettlementReport.Application.SettlementReports_v2.MeasurementsReport(
            SystemClock.Instance,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new ReportRequestId(Guid.NewGuid().ToString()),
            new MeasurementsReportRequestDto(requestFilterDto, null, null));

        // act
        await target.AddOrUpdateAsync(report);

        // assert
        await using var readContext = _databaseManager.CreateDbContext();
        var actual = await readContext.SettlementReports.SingleOrDefaultAsync(x => x.Id == report.Id);

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
    }
}
