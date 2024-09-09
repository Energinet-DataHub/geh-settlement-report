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
using Energinet.DataHub.SettlementReport.Interfaces.Helpers;
using Energinet.DataHub.SettlementReport.Interfaces.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Moq;
using Xunit;

namespace Energinet.DataHub.SettlementReport.UnitTests.Handlers;

public class RequestSettlementReportHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_ReturnsJobId()
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
        var initializerMock = new Mock<ISettlementReportInitializeHandler>();
        var jobHelperMock = new Mock<IDatabricksJobsHelper>();
        var jobRunId = new JobRunId(Random.Shared.NextInt64());
        jobHelperMock
            .Setup(x => x.RunSettlementReportsJobAsync(It.IsAny<SettlementReportRequestDto>()))
            .ReturnsAsync(jobRunId);

        var command = new RequestSettlementReportCommand(request, Guid.NewGuid(), Guid.NewGuid(), true, null);
        var handler = new RequestSettlementReportHandler(jobHelperMock.Object, initializerMock.Object);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(jobRunId, result);
    }
}
