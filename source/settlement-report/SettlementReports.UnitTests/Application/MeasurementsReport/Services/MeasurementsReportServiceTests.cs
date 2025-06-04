using Energinet.DataHub.Reports.Abstractions.Model;
using Energinet.DataHub.Reports.Abstractions.Model.MeasurementsReport;
using Energinet.DataHub.Reports.Application.MeasurementsReport;
using Energinet.DataHub.Reports.Application.MeasurementsReport.Services;
using Energinet.DataHub.Reports.Interfaces.Helpers;
using FluentAssertions;
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
        var measurementsReport = CreateMeasurementsReport(new JobRunId(Random.Shared.NextInt64()));
        var measurementsReportRepositoryMock = new Mock<IMeasurementsReportRepository>();
        measurementsReportRepositoryMock.Setup(x => x.GetByRequestIdAsync(measurementsReport.RequestId))
            .ReturnsAsync(measurementsReport);
        var jobHelperMock = new Mock<IMeasurementsReportDatabricksJobsHelper>();
        jobHelperMock
            .Setup(x => x.CancelAsync(measurementsReport.JobRunId!.Value))
            .Returns(Task.CompletedTask);
        var sut = new MeasurementsReportService(measurementsReportRepositoryMock.Object, jobHelperMock.Object);

        // Act & Assert
        await sut.CancelAsync(new ReportRequestId(measurementsReport.RequestId), measurementsReport.UserId);
    }

    [Fact]
    public async Task CancelRequest_WhenReportMarkedAsFailed_ThrowsException()
    {
        // Arrange
        var measurementsReport = CreateMeasurementsReport(new JobRunId(Random.Shared.NextInt64()));
        measurementsReport.MarkAsFailed();

        var measurementsReportRepositoryMock = new Mock<IMeasurementsReportRepository>();
        measurementsReportRepositoryMock.Setup(x => x.GetByRequestIdAsync(measurementsReport.RequestId))
            .ReturnsAsync(measurementsReport);
        var jobHelperMock = new Mock<IMeasurementsReportDatabricksJobsHelper>();
        var sut = new MeasurementsReportService(measurementsReportRepositoryMock.Object, jobHelperMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.CancelAsync(new ReportRequestId(measurementsReport.RequestId), measurementsReport.UserId));
        exception.Message.Should().Be($"Can't cancel a report with status: Failed.");
    }

    [Fact]
    public async Task CancelRequest_WhenReportStartedByAnotherUser_ThrowsException()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var measurementsReport = CreateMeasurementsReport(new JobRunId(Random.Shared.NextInt64()));
        var measurementsReportRepositoryMock = new Mock<IMeasurementsReportRepository>();
        measurementsReportRepositoryMock.Setup(x => x.GetByRequestIdAsync(measurementsReport.RequestId))
            .ReturnsAsync(measurementsReport);
        var jobHelperMock = new Mock<IMeasurementsReportDatabricksJobsHelper>();
        var sut = new MeasurementsReportService(measurementsReportRepositoryMock.Object, jobHelperMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.CancelAsync(new ReportRequestId(measurementsReport.RequestId), otherUserId));
        exception.Message.Should().Be("UserId does not match. Only the user that started the report can cancel it.");
    }

    [Fact]
    public async Task CancelRequest_WhenJobRunIdIsMissing_ThrowsException()
    {
        // Arrange
        var measurementsReport = CreateMeasurementsReport(null);
        var measurementsReportRepositoryMock = new Mock<IMeasurementsReportRepository>();
        measurementsReportRepositoryMock.Setup(x => x.GetByRequestIdAsync(measurementsReport.RequestId))
            .ReturnsAsync(measurementsReport);
        var jobHelperMock = new Mock<IMeasurementsReportDatabricksJobsHelper>();
        var sut = new MeasurementsReportService(measurementsReportRepositoryMock.Object, jobHelperMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.CancelAsync(new ReportRequestId(measurementsReport.RequestId), measurementsReport.UserId));
        exception.Message.Should().Be("Report does not have a JobRunId.");
    }

    private static Reports.Application.MeasurementsReport.MeasurementsReport CreateMeasurementsReport(JobRunId? jobRunId)
    {
        var gridAreas = new List<string> { "101", "102" };
        var request = new MeasurementsReportRequestDto(
            new MeasurementsReportRequestFilterDto(
                gridAreas,
                DateTimeOffset.Now,
                DateTimeOffset.Now));
        var reportRequestId = new ReportRequestId($"{Random.Shared.NextInt64()}");
        var userId = Guid.NewGuid();

        var clockMock = new Mock<IClock>();
        clockMock
            .Setup(clock => clock.GetCurrentInstant())
            .Returns(Instant.FromUtc(2021, 1, 1, 0, 0));

        return jobRunId != null
            ? new Reports.Application.MeasurementsReport.MeasurementsReport(
                clockMock.Object,
                userId,
                Guid.NewGuid(),
                jobRunId,
                reportRequestId,
                request)
            : new Reports.Application.MeasurementsReport.MeasurementsReport(
                clockMock.Object,
                userId,
                Guid.NewGuid(),
                reportRequestId,
                request);
    }
}
