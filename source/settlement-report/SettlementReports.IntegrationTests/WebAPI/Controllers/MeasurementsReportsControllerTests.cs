using Energinet.DataHub.Core.App.Common.Abstractions.Users;
using Energinet.DataHub.Reports.Abstractions.Model.MeasurementsReport;
using Energinet.DataHub.Reports.Application.MeasurementsReport.Handlers;
using Energinet.DataHub.Reports.Application.MeasurementsReport.Services;
using Energinet.DataHub.Reports.Common.Infrastructure.Security;
using Energinet.DataHub.Reports.WebAPI;
using Energinet.DataHub.Reports.WebAPI.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Xunit;

namespace Energinet.DataHub.Reports.IntegrationTests.WebAPI.Controllers;

public class MeasurementsReportsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly string _accessibleGridAreaCode = "803";

    private readonly FrontendUser _allowedFrontendUser = new(
        Guid.NewGuid(),
        false,
        new FrontendActor(
            Guid.NewGuid(),
            "actor-number",
            FrontendActorMarketRole.EnergySupplier,
            [_accessibleGridAreaCode]));

    [Theory]
    [MemberData(nameof(GetForbiddenFrontendUsers))]
    public async Task GivenReportRequest_WhenForbiddenRequest_ThenResponseIsForbiddenRequest(
        FrontendUser unsupportedFrontendUser)
    {
        // Arrange
        var userContextMock = new Mock<IUserContext<FrontendUser>>();
        userContextMock.Setup(x => x.CurrentUser).Returns(unsupportedFrontendUser);
        var forbiddenRequest = new MeasurementsReportRequestDto(
            new MeasurementsReportRequestFilterDto(
                new List<string> { "123" }, // Forbidden grid area code
                new DateTimeOffset(2022, 1, 11, 23, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2022, 1, 12, 23, 0, 0, TimeSpan.Zero),
                null));

        var sut = CreateSut(userContextMock: userContextMock);

        // Act
        var actual = await sut.RequestMeasurementsReport(forbiddenRequest);

        // Assert
        actual.Result.Should().BeOfType<ForbidResult>();
    }

    [Theory]
    [MemberData(nameof(GetBadRequests))]
    public async Task GivenReportRequest_WhenBadRequest_ThenResponseIsBadRequest(
        MeasurementsReportRequestDto badRequest)
    {
        // Arrange
        var userContextMock = new Mock<IUserContext<FrontendUser>>();
        userContextMock.Setup(x => x.CurrentUser).Returns(_allowedFrontendUser);

        var sut = CreateSut(userContextMock: userContextMock);

        // Act
        var actual = await sut.RequestMeasurementsReport(badRequest);

        // Assert
        actual.Result.Should().BeOfType<BadRequestResult>();
    }

    public static MeasurementsReportsController CreateSut(
        Mock<IRequestMeasurementsReportHandler>? requestMeasurementsReportHandlerMock = null,
        Mock<IMeasurementsReportFileService>? measurementsReportFileServiceMock = null,
        Mock<IListMeasurementsReportService>? listMeasurementsReportServiceMock = null,
        Mock<IUserContext<FrontendUser>>? userContextMock = null)
    {
        requestMeasurementsReportHandlerMock ??= new Mock<IRequestMeasurementsReportHandler>();
        measurementsReportFileServiceMock ??= new Mock<IMeasurementsReportFileService>();
        listMeasurementsReportServiceMock ??= new Mock<IListMeasurementsReportService>();
        userContextMock ??= new Mock<IUserContext<FrontendUser>>();

        return new MeasurementsReportsController(
            requestMeasurementsReportHandlerMock.Object,
            measurementsReportFileServiceMock.Object,
            listMeasurementsReportServiceMock.Object,
            userContextMock.Object);
    }

    public static IEnumerable<object[]> GetForbiddenFrontendUsers()
    {
        // Forbidden actor role
        foreach (var unsupportedActorRole in
                 new[]
                 {
                     FrontendActorMarketRole.DataHubAdministrator,
                     FrontendActorMarketRole.SystemOperator,
                     FrontendActorMarketRole.Other,
                 })
        {
            yield return
            [
                new FrontendUser(
                    Guid.NewGuid(),
                    false,
                    new FrontendActor(Guid.NewGuid(), "actor-number", unsupportedActorRole, [_accessibleGridAreaCode])),
            ];
        }

        // No access to requested grid area
        foreach (var supportedActorRole in
                 new[] { FrontendActorMarketRole.EnergySupplier, FrontendActorMarketRole.GridAccessProvider })
        {
            yield return
            [
                new FrontendUser(
                    Guid.NewGuid(),
                    false,
                    new FrontendActor(Guid.NewGuid(), "actor-number", supportedActorRole, [])),
            ];
        }
    }

    public static IEnumerable<object[]> GetBadRequests()
    {
        // Period end before period start
        yield return
        [
            new MeasurementsReportRequestDto(
                new MeasurementsReportRequestFilterDto(
                    new List<string> { _accessibleGridAreaCode },
                    new DateTimeOffset(2022, 1, 11, 23, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2022, 1, 10, 23, 0, 0, TimeSpan.Zero),
                    null)),
        ];
        // Period end equals period start
        yield return
        [
            new MeasurementsReportRequestDto(
                new MeasurementsReportRequestFilterDto(
                    new List<string> { _accessibleGridAreaCode },
                    new DateTimeOffset(2022, 1, 11, 23, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2022, 1, 11, 23, 0, 0, TimeSpan.Zero),
                    null)),
        ];
        // Period exceeds one month
        yield return
        [
            new MeasurementsReportRequestDto(
                new MeasurementsReportRequestFilterDto(
                    new List<string> { _accessibleGridAreaCode },
                    new DateTimeOffset(2022, 1, 11, 23, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2022, 2, 12, 23, 0, 0, TimeSpan.Zero),
                    null)),
        ];
    }
}
