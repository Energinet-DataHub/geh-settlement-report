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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.SettlementReport.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.SettlementReport.Infrastructure.SqlStatements.Mappers;
using Energinet.DataHub.SettlementReport.Interfaces.CalculationResults.Model;
using Xunit;

namespace Energinet.DataHub.SettlementReport.UnitTests.Infrastructure.SqlStatements.Mappers;

public class SettlementMethodMapperTests
{
    [Theory]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.Production, null!)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.NonProfiledConsumption, SettlementMethod.NonProfiled)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.NetExchangePerNeighboringGridArea, null!)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.NetExchangePerGridArea, null!)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.FlexConsumption, SettlementMethod.Flex)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.GridLoss, null!)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.NegativeGridLoss, null!)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.PositiveGridLoss, SettlementMethod.Flex)]
    public void FromDeltaTableValue_ReturnsValidSettlementMethod(string deltaValue, SettlementMethod? expected)
    {
        // Act
        var actual = SettlementMethodMapper.FromTimeSeriesTypeDeltaTableValue(deltaValue);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void FromDeltaTableValue_WhenInvalidDeltaTableValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidDeltaTableValue = Guid.NewGuid().ToString();

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => SettlementMethodMapper.FromDeltaTableValue(invalidDeltaTableValue));
    }
}
