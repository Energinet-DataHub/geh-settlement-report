﻿// Copyright 2020 Energinet DataHub A/S
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

using Energinet.DataHub.SettlementReport.Application.SettlementReports_v2;
using Energinet.DataHub.SettlementReport.Interfaces.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models;
using Energinet.DataHub.SettlementReport.Interfaces.SettlementReports_v2.Models.SettlementReport;
using Moq;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.SettlementReport.UnitTests.Application.SettlementReports_v2;

public class SettlementReportFileRequestHandlerTests
{
    private readonly Dictionary<string, CalculationId?> _gridAreaCodes = new Dictionary<string, CalculationId?>() { { "373", new CalculationId(Guid.NewGuid()) } };
    private readonly Instant _startDate = Instant.FromUtc(2022, 1, 1, 0, 0, 0);
    private readonly Instant _endDate = Instant.FromUtc(2022, 1, 31, 0, 0, 0);
    private readonly string _fileName = "FILENAME";
    private readonly string _glnNumber = "123456789";
    private readonly ReportRequestId _requestId = new ReportRequestId(Guid.NewGuid().ToString());

    [Fact]
    public async Task RequestFile_ForEnergySupplier_GLNShouldAppearInFilename()
    {
        // Arrange
        await using var memoryStream = new MemoryStream();
        var settlementReportFileGenerator = new Mock<ISettlementReportFileGenerator>();
        var settlementReportFileGeneratorFactory = new Mock<ISettlementReportFileGeneratorFactory>();
        var settlementReportFileRepository = new Mock<ISettlementReportFileRepository>();

        settlementReportFileGeneratorFactory.Setup(x => x.Create(It.IsAny<SettlementReportFileContent>()))
            .Returns(settlementReportFileGenerator.Object);
        settlementReportFileRepository.Setup(x => x.OpenForWritingAsync(It.IsAny<ReportRequestId>(), It.IsAny<string>()))
            .ReturnsAsync(memoryStream);

        var filter = new SettlementReportRequestFilterDto(
            _gridAreaCodes,
            _startDate.ToDateTimeOffset(),
            _endDate.ToDateTimeOffset(),
            CalculationType.WholesaleFixing,
            _glnNumber,
            null);

        var fileRequest = new SettlementReportFileRequestDto(
            _requestId,
            SettlementReportFileContent.ChargePrice,
            new SettlementReportPartialFileInfo(_fileName, true),
            filter,
            1);

        var sut = new SettlementReportFileRequestHandler(
            settlementReportFileGeneratorFactory.Object,
            settlementReportFileRepository.Object);

        // Act
        var resultGeneratedSettlementReportFile = await sut.RequestFileAsync(fileRequest, new SettlementReportRequestedByActor(MarketRole.EnergySupplier, null));
        var resultedFileName = resultGeneratedSettlementReportFile.FileInfo.FileName;

        // Assert
        Assert.Contains(_fileName, resultedFileName);
        Assert.Contains(_glnNumber, resultedFileName);
        Assert.Contains("DDQ", resultedFileName);
        Assert.Contains($"{_startDate:dd-MM-yyyy}", resultedFileName);
        Assert.Contains($"{_endDate:dd-MM-yyyy}", resultedFileName);
    }

    [Fact]
    public async Task RequestFile_ForGridAccessProvider_GLNShouldAppearInFilename()
    {
        // Arrange
        await using var memoryStream = new MemoryStream();
        var settlementReportFileGenerator = new Mock<ISettlementReportFileGenerator>();
        var settlementReportFileGeneratorFactory = new Mock<ISettlementReportFileGeneratorFactory>();
        var settlementReportFileRepository = new Mock<ISettlementReportFileRepository>();

        settlementReportFileGeneratorFactory.Setup(x => x.Create(It.IsAny<SettlementReportFileContent>()))
            .Returns(settlementReportFileGenerator.Object);
        settlementReportFileRepository.Setup(x => x.OpenForWritingAsync(It.IsAny<ReportRequestId>(), It.IsAny<string>()))
            .ReturnsAsync(memoryStream);

        var filter = new SettlementReportRequestFilterDto(
            _gridAreaCodes,
            _startDate.ToDateTimeOffset(),
            _endDate.ToDateTimeOffset(),
            CalculationType.WholesaleFixing,
            null,
            null);

        var fileRequest = new SettlementReportFileRequestDto(
            _requestId,
            SettlementReportFileContent.ChargePrice,
            new SettlementReportPartialFileInfo(_fileName, true),
            filter,
            1);

        var sut = new SettlementReportFileRequestHandler(
            settlementReportFileGeneratorFactory.Object,
            settlementReportFileRepository.Object);

        // Act
        var resultGeneratedSettlementReportFile = await sut.RequestFileAsync(fileRequest, new SettlementReportRequestedByActor(MarketRole.GridAccessProvider, _glnNumber));
        var resultedFileName = resultGeneratedSettlementReportFile.FileInfo.FileName;

        // Assert
        Assert.Contains(_fileName, resultedFileName);
        Assert.Contains(_glnNumber, resultedFileName);
        Assert.Contains("DDM", resultedFileName);
        Assert.Contains($"{_startDate:dd-MM-yyyy}", resultedFileName);
        Assert.Contains($"{_endDate:dd-MM-yyyy}", resultedFileName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RequestFile_NoEnergySupplier_ShouldNotAppearInFilename(string? energySupplier)
    {
        // Arrange
        await using var memoryStream = new MemoryStream();
        var settlementReportFileGenerator = new Mock<ISettlementReportFileGenerator>();
        var settlementReportFileGeneratorFactory = new Mock<ISettlementReportFileGeneratorFactory>();
        var settlementReportFileRepository = new Mock<ISettlementReportFileRepository>();

        settlementReportFileGeneratorFactory.Setup(x => x.Create(It.IsAny<SettlementReportFileContent>()))
            .Returns(settlementReportFileGenerator.Object);
        settlementReportFileRepository.Setup(x => x.OpenForWritingAsync(It.IsAny<ReportRequestId>(), It.IsAny<string>()))
            .ReturnsAsync(memoryStream);

        var filter = new SettlementReportRequestFilterDto(
            _gridAreaCodes,
            _startDate.ToDateTimeOffset(),
            _endDate.ToDateTimeOffset(),
            CalculationType.WholesaleFixing,
            energySupplier,
            null);

        var fileRequest = new SettlementReportFileRequestDto(
            _requestId,
            SettlementReportFileContent.ChargePrice,
            new SettlementReportPartialFileInfo(_fileName, true),
            filter,
            1);

        var sut = new SettlementReportFileRequestHandler(
            settlementReportFileGeneratorFactory.Object,
            settlementReportFileRepository.Object);

        // Act
        var resultGeneratedSettlementReportFile = await sut.RequestFileAsync(fileRequest, new SettlementReportRequestedByActor(MarketRole.EnergySupplier, null));
        var resultedFileName = resultGeneratedSettlementReportFile.FileInfo.FileName;

        // Assert
        Assert.Contains($"{_fileName}_DDM_{_startDate:dd-MM-yyyy}", resultedFileName);
    }
}
