using System.Collections.ObjectModel;
using Energinet.DataHub.Reports.Application.MeasurementsReport.Services;
using Energinet.DataHub.Reports.Application.SettlementReports_v2;
using Energinet.DataHub.Reports.Interfaces.Helpers;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.MeasurementsReport;
using Energinet.DataHub.Reports.Interfaces.SettlementReports_v2.Models.SettlementReport;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Reports.UnitTests.Application.MeasurementsReport.Services;

public class MeasurementsReportServiceTests
{
    [Fact]
    public async Task CancelRequest_Completes_Successfully()
    {
        // Arrange
        var gridAreas = new ReadOnlyDictionary<string, CalculationId?>(new Dictionary<string, CalculationId?>
        {
            { "101", new CalculationId(Guid.NewGuid()) },
            { "102", new CalculationId(Guid.NewGuid()) },
        });
        var request = new MeasurementsReportRequestDto(
            new MeasurementsReportRequestFilterDto(
                gridAreas,
                DateTimeOffset.Now,
                DateTimeOffset.Now));
        var reportRequestId = new ReportRequestId($"{Random.Shared.NextInt64()}");
        var jobRunId = new JobRunId(Random.Shared.NextInt64());
        var userId = Guid.NewGuid();

        var clockMock = new Mock<IClock>();
        clockMock
            .Setup(clock => clock.GetCurrentInstant())
            .Returns(Instant.FromUtc(2021, 1, 1, 0, 0));

        var measurementsReport =
            new Reports.Application.SettlementReports_v2.MeasurementsReport(
                clockMock.Object,
                userId,
                Guid.NewGuid(),
                jobRunId,
                reportRequestId,
                request);

        var measurementsReportRepositoryMock = new Mock<IMeasurementsReportRepository>();
        measurementsReportRepositoryMock.Setup(x => x.GetAsync(reportRequestId.Id))
            .ReturnsAsync(measurementsReport);
        var jobHelperMock = new Mock<IMeasurementsReportDatabricksJobsHelper>();
        jobHelperMock
            .Setup(x => x.CancelAsync(measurementsReport.JobRunId!.Value))
            .Returns(Task.CompletedTask);
        var sut = new MeasurementsReportService(measurementsReportRepositoryMock.Object, jobHelperMock.Object);

        // Act & Assert
        await sut.CancelAsync(reportRequestId, userId);
    }

    [Fact]
    public async Task CancelRequest_Completes_Unsuccessfully()
    {
        // Arrange
        var reportRequestId = CreateMeasurementsReport(out var userId, out var measurementsReport);

        measurementsReport.MarkAsFailed();

        var measurementsReportRepositoryMock = new Mock<IMeasurementsReportRepository>();
        measurementsReportRepositoryMock.Setup(x => x.GetAsync(reportRequestId.Id))
            .ReturnsAsync(measurementsReport);
        var jobHelperMock = new Mock<IMeasurementsReportDatabricksJobsHelper>();
        jobHelperMock
            .Setup(x => x.CancelAsync(measurementsReport.JobRunId!.Value))
            .Returns(Task.CompletedTask);
        var sut = new MeasurementsReportService(measurementsReportRepositoryMock.Object, jobHelperMock.Object);

        // Act & Assert
        await sut.CancelAsync(reportRequestId, userId);
    }

    private static MeasurementsReport CreateMeasurementsReport(out Guid userId, out Reports.Application.SettlementReports_v2.MeasurementsReport measurementsReport)
    {
        var gridAreas = new ReadOnlyDictionary<string, CalculationId?>(new Dictionary<string, CalculationId?>
        {
            { "101", new CalculationId(Guid.NewGuid()) },
            { "102", new CalculationId(Guid.NewGuid()) },
        });
        var request = new MeasurementsReportRequestDto(
            new MeasurementsReportRequestFilterDto(
                gridAreas,
                DateTimeOffset.Now,
                DateTimeOffset.Now));
        var reportRequestId = new ReportRequestId($"{Random.Shared.NextInt64()}");
        var jobRunId = new JobRunId(Random.Shared.NextInt64());
        userId = Guid.NewGuid();

        var clockMock = new Mock<IClock>();
        clockMock
            .Setup(clock => clock.GetCurrentInstant())
            .Returns(Instant.FromUtc(2021, 1, 1, 0, 0));

        measurementsReport = new Reports.Application.SettlementReports_v2.MeasurementsReport(
            clockMock.Object,
            userId,
            Guid.NewGuid(),
            jobRunId,
            reportRequestId,
            request);

        return measurementsReport;
    }
}
