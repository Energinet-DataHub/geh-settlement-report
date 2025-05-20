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

using System.Collections.ObjectModel;
using Energinet.DataHub.SettlementReport.Application.Commands;
using Energinet.DataHub.SettlementReport.Application.Handlers;
using Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.Helpers;
using Energinet.DataHub.SettlementReport.Interfaces.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.SettlementReport.UnitTests.Handlers;

public class CancelSettlementReportJobHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_CompletesSuccessfuly()
    {
        // Arrange
        var gridAreas = new ReadOnlyDictionary<string, CalculationId?>(new Dictionary<string, CalculationId?>
        {
            { "101", new CalculationId(Guid.NewGuid()) },
            { "102", new CalculationId(Guid.NewGuid()) },
        });
        var request = new SettlementReportRequestDto(
            true,
            true,
            true,
            true,
            new SettlementReportRequestFilterDto(
                gridAreas,
                DateTimeOffset.Now,
                DateTimeOffset.Now,
                CalculationType.WholesaleFixing,
                null,
                null));
        var settlementReportRequestId = new SettlementReportRequestId($"{Random.Shared.NextInt64()}");
        var jobRunId = new JobRunId(Random.Shared.NextInt64());
        var userId = Guid.NewGuid();

        var clockMock = new Mock<IClock>();
        clockMock
            .Setup(clock => clock.GetCurrentInstant())
            .Returns(Instant.FromUtc(2021, 1, 1, 0, 0));
        var settlementReport =
            new SettlementReport.Application.SettlementReports_v2.SettlementReport(
                clockMock.Object,
                userId,
                Guid.NewGuid(),
                false,
                jobRunId,
                settlementReportRequestId,
                request);

        var jobHelperMock = new Mock<IDatabricksJobsHelper>();
        var repository = new Mock<ISettlementReportRepository>();
        repository
            .Setup(x => x.GetAsync(settlementReportRequestId.Id))
            .ReturnsAsync(settlementReport);
        var command = new CancelSettlementReportCommand(settlementReportRequestId, userId);
        var handler = new CancelSettlementReportJobHandler(jobHelperMock.Object, repository.Object);

        // Act
        await handler.HandleAsync(command);

        // Assert
    }

    [Fact]
    public async Task Handle_ReturnsFailedReport_ThrowsException()
    {
        // Arrange
        var gridAreas = new ReadOnlyDictionary<string, CalculationId?>(new Dictionary<string, CalculationId?>
        {
            { "101", new CalculationId(Guid.NewGuid()) },
            { "102", new CalculationId(Guid.NewGuid()) },
        });
        var request = new SettlementReportRequestDto(
            true,
            true,
            true,
            true,
            new SettlementReportRequestFilterDto(
                gridAreas,
                DateTimeOffset.Now,
                DateTimeOffset.Now,
                CalculationType.WholesaleFixing,
                null,
                null));
        var settlementReportRequestId = new SettlementReportRequestId($"{Random.Shared.NextInt64()}");
        var jobRunId = new JobRunId(Random.Shared.NextInt64());
        var userId = Guid.NewGuid();

        var clockMock = new Mock<IClock>();
        clockMock
            .Setup(clock => clock.GetCurrentInstant())
            .Returns(Instant.FromUtc(2021, 1, 1, 0, 0));
        var settlementReport =
            new SettlementReport.Application.SettlementReports_v2.SettlementReport(
                clockMock.Object,
                userId,
                Guid.NewGuid(),
                false,
                jobRunId,
                settlementReportRequestId,
                request);
        settlementReport.MarkAsFailed();

        var jobHelperMock = new Mock<IDatabricksJobsHelper>();
        var repository = new Mock<ISettlementReportRepository>();
        repository
            .Setup(x => x.GetAsync(settlementReportRequestId.Id))
            .ReturnsAsync(settlementReport);
        var command = new CancelSettlementReportCommand(settlementReportRequestId, userId);
        var handler = new CancelSettlementReportJobHandler(jobHelperMock.Object, repository.Object);

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task Handle_ReturnsReportStartedByAnotherUser_ThrowsException()
    {
        // Arrange
        var gridAreas = new ReadOnlyDictionary<string, CalculationId?>(new Dictionary<string, CalculationId?>
        {
            { "101", new CalculationId(Guid.NewGuid()) },
            { "102", new CalculationId(Guid.NewGuid()) },
        });
        var request = new SettlementReportRequestDto(
            true,
            true,
            true,
            true,
            new SettlementReportRequestFilterDto(
                gridAreas,
                DateTimeOffset.Now,
                DateTimeOffset.Now,
                CalculationType.WholesaleFixing,
                null,
                null));
        var settlementReportRequestId = new SettlementReportRequestId($"{Random.Shared.NextInt64()}");
        var jobRunId = new JobRunId(Random.Shared.NextInt64());
        var userId = Guid.NewGuid();

        var clockMock = new Mock<IClock>();
        clockMock
            .Setup(clock => clock.GetCurrentInstant())
            .Returns(Instant.FromUtc(2021, 1, 1, 0, 0));
        var settlementReport =
            new SettlementReport.Application.SettlementReports_v2.SettlementReport(
                clockMock.Object,
                Guid.NewGuid(),
                Guid.NewGuid(),
                false,
                jobRunId,
                settlementReportRequestId,
                request);

        var jobHelperMock = new Mock<IDatabricksJobsHelper>();
        var repository = new Mock<ISettlementReportRepository>();
        repository
            .Setup(x => x.GetAsync(settlementReportRequestId.Id))
            .ReturnsAsync(settlementReport);
        var command = new CancelSettlementReportCommand(settlementReportRequestId, userId);
        var handler = new CancelSettlementReportJobHandler(jobHelperMock.Object, repository.Object);

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task Handle_ReturnsReportWithoutJobId_ThrowsException()
    {
        // Arrange
        var gridAreas = new ReadOnlyDictionary<string, CalculationId?>(new Dictionary<string, CalculationId?>
        {
            { "101", new CalculationId(Guid.NewGuid()) },
            { "102", new CalculationId(Guid.NewGuid()) },
        });
        var request = new SettlementReportRequestDto(
            true,
            true,
            true,
            true,
            new SettlementReportRequestFilterDto(
                gridAreas,
                DateTimeOffset.Now,
                DateTimeOffset.Now,
                CalculationType.WholesaleFixing,
                null,
                null));
        var settlementReportRequestId = new SettlementReportRequestId($"{Random.Shared.NextInt64()}");
        var jobRunId = new JobRunId(Random.Shared.NextInt64());
        var userId = Guid.NewGuid();

        var clockMock = new Mock<IClock>();
        clockMock
            .Setup(clock => clock.GetCurrentInstant())
            .Returns(Instant.FromUtc(2021, 1, 1, 0, 0));
        var settlementReport =
            new SettlementReport.Application.SettlementReports_v2.SettlementReport(
                clockMock.Object,
                Guid.NewGuid(),
                Guid.NewGuid(),
                false,
                settlementReportRequestId,
                request);

        var jobHelperMock = new Mock<IDatabricksJobsHelper>();
        var repository = new Mock<ISettlementReportRepository>();
        repository
            .Setup(x => x.GetAsync(settlementReportRequestId.Id))
            .ReturnsAsync(settlementReport);
        var command = new CancelSettlementReportCommand(settlementReportRequestId, userId);
        var handler = new CancelSettlementReportJobHandler(jobHelperMock.Object, repository.Object);

        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
    }
}
