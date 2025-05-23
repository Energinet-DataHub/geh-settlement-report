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

using Energinet.DataHub.SettlementReport.Infrastructure.SqlStatements.DeltaTableConstants;
using Energinet.DataHub.SettlementReport.Infrastructure.SqlStatements.Mappers.EnergyResult;
using Energinet.DataHub.SettlementReport.Interfaces.CalculationResults.Model.EnergyResults;
using Xunit;

namespace Energinet.DataHub.SettlementReport.UnitTests.Infrastructure.SqlStatements.Mappers.EnergyResult;

public class AggregationLevelMapperTests
{
    [Theory]
    [InlineData(TimeSeriesType.Production)]
    [InlineData(TimeSeriesType.FlexConsumption)]
    [InlineData(TimeSeriesType.NonProfiledConsumption)]
    public void ToDeltaTableValue_WhenNoActorsSpecified_ReturnsExpectedAggLevel(TimeSeriesType type)
    {
        // Act
        const string expected = DeltaTableAggregationLevel.GridArea;
        var actual = AggregationLevelMapper.ToDeltaTableValue(type, null, null);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(TimeSeriesType.Production)]
    [InlineData(TimeSeriesType.FlexConsumption)]
    [InlineData(TimeSeriesType.NonProfiledConsumption)]
    public void ToDeltaTableValue_WhenEnergySupplierIsNotNull_ReturnsExpectedAggLevel(TimeSeriesType type)
    {
        // Act
        const string expected = DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea;
        var actual = AggregationLevelMapper.ToDeltaTableValue(type, "someEnergySupplier", null);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(TimeSeriesType.Production)]
    [InlineData(TimeSeriesType.FlexConsumption)]
    [InlineData(TimeSeriesType.NonProfiledConsumption)]
    public void ToDeltaTableValue_WhenBalanceResponsibleIsNotNull_ReturnsExpectedAggLevel(TimeSeriesType type)
    {
        // Act
        const string expected = DeltaTableAggregationLevel.BalanceResponsibleAndGridArea;
        var actual = AggregationLevelMapper.ToDeltaTableValue(type, null, "somBalanceResponsible");

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(TimeSeriesType.Production)]
    [InlineData(TimeSeriesType.FlexConsumption)]
    [InlineData(TimeSeriesType.NonProfiledConsumption)]
    public void ToDeltaTableValue_WhenNeitherEnergySupplierAndBalanceResponsibleIsNull_ReturnsExpectedAggLevel(TimeSeriesType type)
    {
        // Act
        const string expected = DeltaTableAggregationLevel.EnergySupplierAndBalanceResponsibleAndGridArea;
        var actual = AggregationLevelMapper.ToDeltaTableValue(type, "someEnergySupplier", "somBalanceResponsible");

        // Assert
        Assert.Equal(expected, actual);
    }
}
