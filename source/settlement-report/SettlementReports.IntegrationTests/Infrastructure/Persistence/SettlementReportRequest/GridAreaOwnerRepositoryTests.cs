﻿using Energinet.DataHub.Reports.Application.Model;
using Energinet.DataHub.Reports.Infrastructure.Contracts;
using Energinet.DataHub.Reports.Infrastructure.Persistence;
using Energinet.DataHub.Reports.Infrastructure.Services;
using Energinet.DataHub.Reports.Test.Core.Fixture.Database;
using Google.Protobuf.WellKnownTypes;
using NodaTime.Extensions;
using Xunit;

namespace Energinet.DataHub.Reports.IntegrationTests.Infrastructure.Persistence.SettlementReportRequest;

public sealed class GridAreaOwnerRepositoryTests : IClassFixture<WholesaleDatabaseFixture<ReportsDatabaseContext>>
{
    private readonly WholesaleDatabaseManager<ReportsDatabaseContext> _databaseManager;

    public GridAreaOwnerRepositoryTests(WholesaleDatabaseFixture<ReportsDatabaseContext> fixture)
    {
        _databaseManager = fixture.DatabaseManager;
    }

    [Theory]
    // fixed owners for grid area 1:2024-01-01, 2:2024-01-15
    // sliding window + grid area code + expected
    [InlineData("2024-02-01", "2024-03-01", "100", "2")]
    [InlineData("2024-01-14", "2024-03-01", "101", "1,2")]
    [InlineData("2024-01-01", "2024-01-14", "102", "1")]
    [InlineData("2024-01-01", "2024-01-15", "103", "1")]
    [InlineData("2024-01-01", "2024-01-16", "104", "1,2")]
    [InlineData("2023-12-30", "2023-12-31", "105", "")]
    [InlineData("2023-12-30", "2024-01-01", "106", "")]
    [InlineData("2023-12-30", "2024-01-02", "107", "1")]
    [InlineData("2023-12-30", "2024-04-02", "108", "1,2")]
    public async Task Get_GridOwnerInPeriod_ReturnsOwner(string from, string to, string gridAreaCode, string expectedOwners)
    {
        // arrange
        await using var dbContext = _databaseManager.CreateDbContext();

        var target = new GridAreaOwnerRepository(dbContext);

        await target.AddAsync(new GridAreaOwnershipAssigned { GridAreaCode = new GridAreaCode(gridAreaCode).Value, ActorNumber = "1", ValidFrom = DateTimeOffset.Parse("2024-01-01").ToTimestamp(), SequenceNumber = 1 });
        await target.AddAsync(new GridAreaOwnershipAssigned { GridAreaCode = new GridAreaCode(gridAreaCode).Value, ActorNumber = "2", ValidFrom = DateTimeOffset.Parse("2024-01-15").ToTimestamp(), SequenceNumber = 1 });

        // act
        var owners = (await target.GetGridAreaOwnersAsync(
                new GridAreaCode(gridAreaCode),
                DateTimeOffset.Parse(from).ToInstant(),
                DateTimeOffset.Parse(to).ToInstant()))
            .Select(x => x.ActorNumber)
            .ToList();

        // assert
        var expected = expectedOwners.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => new ActorNumber(x)).ToList();

        Assert.Equal(expected.Count, owners.Count);

        foreach (var expectedOwner in expected)
        {
            Assert.Contains(expectedOwner, owners);
        }
    }
}
