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

using Energinet.DataHub.Core.TestCommon.AutoFixture.Attributes;
using Energinet.DataHub.SettlementReport.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.SettlementReport.Infrastructure.SqlStatements.Mappers.EnergyResult;
using Energinet.DataHub.SettlementReport.Interfaces.CalculationResults.Model.EnergyResults;
using Xunit;

namespace Energinet.DataHub.SettlementReport.UnitTests.Infrastructure.SqlStatements.Mappers.EnergyResult;

public class TimeSeriesTypeMapperTests
{
    [Theory]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.Production, TimeSeriesType.Production)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.NonProfiledConsumption, TimeSeriesType.NonProfiledConsumption)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.NetExchangePerNeighboringGridArea, TimeSeriesType.NetExchangePerNeighboringGa)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.NetExchangePerGridArea, TimeSeriesType.NetExchangePerGa)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.FlexConsumption, TimeSeriesType.FlexConsumption)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.GridLoss, TimeSeriesType.GridLoss)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.NegativeGridLoss, TimeSeriesType.NegativeGridLoss)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.PositiveGridLoss, TimeSeriesType.PositiveGridLoss)]
    [InlineAutoMoqData(DeltaTableTimeSeriesType.TotalConsumption, TimeSeriesType.TotalConsumption)]
    public void FromDeltaTableValue_ReturnsValidTimeSeriesType(string deltaValue, TimeSeriesType? expected)
    {
        // Act
        var actual = TimeSeriesTypeMapper.FromDeltaTableValue(deltaValue);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(TimeSeriesType.FlexConsumption, DeltaTableTimeSeriesType.FlexConsumption)]
    [InlineData(TimeSeriesType.Production, DeltaTableTimeSeriesType.Production)]
    [InlineData(TimeSeriesType.NonProfiledConsumption, DeltaTableTimeSeriesType.NonProfiledConsumption)]
    [InlineData(TimeSeriesType.NetExchangePerGa, DeltaTableTimeSeriesType.NetExchangePerGridArea)]
    [InlineData(TimeSeriesType.NetExchangePerNeighboringGa, DeltaTableTimeSeriesType.NetExchangePerNeighboringGridArea)]
    [InlineData(TimeSeriesType.GridLoss, DeltaTableTimeSeriesType.GridLoss)]
    [InlineData(TimeSeriesType.NegativeGridLoss, DeltaTableTimeSeriesType.NegativeGridLoss)]
    [InlineData(TimeSeriesType.PositiveGridLoss, DeltaTableTimeSeriesType.PositiveGridLoss)]
    [InlineData(TimeSeriesType.TotalConsumption, DeltaTableTimeSeriesType.TotalConsumption)]
    public void ToDeltaTableValue_ReturnsExpectedString(TimeSeriesType type, string expected)
    {
        // Act
        var actual = TimeSeriesTypeMapper.ToDeltaTableValue(type);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void FromDeltaTableValue_WhenInvalidDeltaTableValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidDeltaTableValue = Guid.NewGuid().ToString();

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => TimeSeriesTypeMapper.FromDeltaTableValue(invalidDeltaTableValue));
    }

    [Fact]
    public void ToDeltaTableValue_WhenInvalidEnumNumberForTimeSeriesType_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidValue = (TimeSeriesType)99;

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => TimeSeriesTypeMapper.ToDeltaTableValue(invalidValue));
    }
}
