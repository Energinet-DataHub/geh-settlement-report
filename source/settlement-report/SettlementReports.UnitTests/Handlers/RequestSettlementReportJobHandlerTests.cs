using System.Collections.ObjectModel;
using Energinet.DataHub.Reports.Application.Services;
using Energinet.DataHub.Reports.Application.SettlementReports.Commands;
using Energinet.DataHub.Reports.Application.SettlementReports.Handlers;
using Energinet.DataHub.Reports.Interfaces;
using Energinet.DataHub.Reports.Interfaces.Helpers;
using Energinet.DataHub.Reports.Interfaces.Models;
using Energinet.DataHub.Reports.Interfaces.Models.SettlementReport;
using Moq;
using Xunit;

namespace Energinet.DataHub.Reports.UnitTests.Handlers;

public class RequestSettlementReportJobHandlerTests
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
        var initializerMock = new Mock<ISettlementReportPersistenceService>();
        var jobHelperMock = new Mock<ISettlementReportDatabricksJobsHelper>();
        var jobRunId = new JobRunId(Random.Shared.NextInt64());
        jobHelperMock
            .Setup(x => x.RunJobAsync(It.IsAny<SettlementReportRequestDto>(), It.IsAny<MarketRole>(), It.IsAny<ReportRequestId>(), It.IsAny<string>()))
            .ReturnsAsync(jobRunId);

        var command = new RequestSettlementReportCommand(request, Guid.NewGuid(), Guid.NewGuid(), true, "1233", MarketRole.EnergySupplier);
        var handler = new RequestSettlementReportJobHandler(jobHelperMock.Object, initializerMock.Object, new Mock<IGridAreaOwnerRepository>().Object);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(jobRunId, result);
    }
}
