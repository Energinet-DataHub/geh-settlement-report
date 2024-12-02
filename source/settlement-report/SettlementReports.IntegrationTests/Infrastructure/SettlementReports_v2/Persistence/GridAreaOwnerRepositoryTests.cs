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
using NodaTime;
using NodaTime.Serialization.Protobuf;
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
    public async Task GetLatestAsync_PastOwnership_ReturnsOwner()
    {
        // Arrange
        var gridAreaCode = new GridAreaCode("111");
        var gridAreaOwnershipAssigned = new GridAreaOwnershipAssigned
        {
            GridAreaCode = gridAreaCode.Value,
            ActorNumber = Guid.NewGuid().ToString(),
            ValidFrom = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromHours(72)).ToTimestamp(),
            SequenceNumber = 1,
        };

        await using var dbContext = _databaseManager.CreateDbContext();
        var target = new GridAreaOwnerRepository(dbContext);
        await target.AddAsync(gridAreaOwnershipAssigned);

        // Act
        var createdGridAreaOwner = await target.GetLatestAsync(new GridAreaCode(gridAreaOwnershipAssigned.GridAreaCode));

        // Assert
        Assert.NotNull(createdGridAreaOwner);
        Assert.Equal(gridAreaOwnershipAssigned.GridAreaCode, createdGridAreaOwner.Code.Value);
        Assert.Equal(gridAreaOwnershipAssigned.ActorNumber, createdGridAreaOwner.ActorNumber.Value);
        Assert.Equal(gridAreaOwnershipAssigned.ValidFrom.ToDateTimeOffset(), createdGridAreaOwner.ValidFrom.ToDateTimeOffset());
    }

    [Fact]
    public async Task GetLatestAsync_FutureOwnership_ReturnsNull()
    {
        // Arrange
        var gridAreaCode = new GridAreaCode("222");
        var gridAreaOwnershipAssigned = new GridAreaOwnershipAssigned
        {
            GridAreaCode = gridAreaCode.Value,
            ActorNumber = Guid.NewGuid().ToString(),
            ValidFrom = SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromHours(72)).ToTimestamp(),
            SequenceNumber = 1,
        };

        await using var dbContext = _databaseManager.CreateDbContext();
        var target = new GridAreaOwnerRepository(dbContext);
        await target.AddAsync(gridAreaOwnershipAssigned);

        // Act
        var createdGridAreaOwner = await target.GetLatestAsync(gridAreaCode);

        // Assert
        Assert.Null(createdGridAreaOwner);
    }

    [Fact]
    public async Task GetLatestAsync_MultipleEvents_ReturnsLatest()
    {
        // Arrange
        var gridAreaCode = new GridAreaCode("333");
        var gridAreaOwnershipAssigned1 = new GridAreaOwnershipAssigned
        {
            GridAreaCode = gridAreaCode.Value,
            ActorNumber = Guid.NewGuid().ToString(),
            ValidFrom = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromHours(72)).ToTimestamp(),
            SequenceNumber = 1,
        };

        var gridAreaOwnershipAssigned2 = new GridAreaOwnershipAssigned
        {
            GridAreaCode = gridAreaOwnershipAssigned1.GridAreaCode,
            ActorNumber = Guid.NewGuid().ToString(),
            ValidFrom = gridAreaOwnershipAssigned1.ValidFrom,
            SequenceNumber = 2,
        };

        var gridAreaOwnershipAssigned3 = new GridAreaOwnershipAssigned
        {
            GridAreaCode = gridAreaOwnershipAssigned1.GridAreaCode,
            ActorNumber = Guid.NewGuid().ToString(),
            ValidFrom = gridAreaOwnershipAssigned1.ValidFrom,
            SequenceNumber = 3,
        };

        await using var dbContext = _databaseManager.CreateDbContext();
        var target = new GridAreaOwnerRepository(dbContext);
        await target.AddAsync(gridAreaOwnershipAssigned2);
        await target.AddAsync(gridAreaOwnershipAssigned3);
        await target.AddAsync(gridAreaOwnershipAssigned1);

        // Act
        var createdGridAreaOwner = await target.GetLatestAsync(gridAreaCode);

        // Assert
        Assert.NotNull(createdGridAreaOwner);
        Assert.Equal(gridAreaOwnershipAssigned3.ActorNumber, createdGridAreaOwner.ActorNumber.Value);
    }
}
