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

using Energinet.DataHub.SettlementReport.Infrastructure.Contracts;
using Energinet.DataHub.SettlementReport.Infrastructure.Model;
using Energinet.DataHub.SettlementReport.Infrastructure.Persistence;
using Energinet.DataHub.SettlementReport.Infrastructure.Services;
using Energinet.DataHub.SettlementReport.Test.Core.Fixture.Database;
using Google.Protobuf.WellKnownTypes;
using NodaTime.Extensions;
using Xunit;

namespace Energinet.DataHub.SettlementReports.IntegrationTests.Infrastructure.SettlementReports_v2.Persistence;

public sealed class GridAreaOwnerRepositoryTests : IClassFixture<WholesaleDatabaseFixture<SettlementReportDatabaseContext>>
{
    private readonly WholesaleDatabaseManager<SettlementReportDatabaseContext> _databaseManager;

    public GridAreaOwnerRepositoryTests(WholesaleDatabaseFixture<SettlementReportDatabaseContext> fixture)
    {
        _databaseManager = fixture.DatabaseManager;
    }

    [Fact]
    public async Task Get_MultipleGridOwnersInPeriod_ReturnsAll()
    {
        // Arrange
        var gridAreaCode = new GridAreaCode("111");
        var actorNumber = "7025915927252";
        var jan = new GridAreaOwnershipAssigned
        {
            GridAreaCode = gridAreaCode.Value,
            ActorNumber = actorNumber,
            ValidFrom = DateTimeOffset.Parse("2024-01-01").ToTimestamp(),
            SequenceNumber = 1,
        };
        var feb = new GridAreaOwnershipAssigned
        {
            GridAreaCode = gridAreaCode.Value,
            ActorNumber = actorNumber,
            ValidFrom = DateTimeOffset.Parse("2024-02-01").ToTimestamp(),
            SequenceNumber = 1,
        };
        var mar = new GridAreaOwnershipAssigned
        {
            GridAreaCode = gridAreaCode.Value,
            ActorNumber = actorNumber,
            ValidFrom = DateTimeOffset.Parse("2024-03-01").ToTimestamp(),
            SequenceNumber = 1,
        };

        await using var dbContext = _databaseManager.CreateDbContext();
        var target = new GridAreaOwnerRepository(dbContext);
        await target.AddAsync(jan);
        await target.AddAsync(feb);
        await target.AddAsync(mar);

        // Act
        var owners = (await target.GetGridAreaOwnersAsync(
            new GridAreaCode(jan.GridAreaCode),
            DateTimeOffset.Parse("2024-01-01").ToInstant(),
            DateTimeOffset.Parse("2024-03-01").AddSeconds(-1).ToInstant()))
            .OrderBy(x => x.ValidFrom).ToList();

        // Assert
        Assert.Equal(2, owners.Count);
        Assert.Equal(DateTimeOffset.Parse("2024-01-01"), owners[0].ValidFrom.ToDateTimeOffset());
        Assert.Equal(DateTimeOffset.Parse("2024-02-01"), owners[1].ValidFrom.ToDateTimeOffset());
    }
}
