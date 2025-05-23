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

using Energinet.DataHub.SettlementReport.Infrastructure.SqlStatements.Mappers.WholesaleResult;
using Energinet.DataHub.SettlementReport.Interfaces.CalculationResults.Model.WholesaleResults;
using Xunit;

namespace Energinet.DataHub.SettlementReport.UnitTests.Infrastructure.SqlStatements.Mappers.WholesaleResult;

public class ResolutionMapperTests
{
    [Theory]
    [InlineData("P1M", Resolution.Month)]
    [InlineData("P1D", Resolution.Day)]
    [InlineData("PT1H", Resolution.Hour)]
    public void FromDeltaTableValue_WhenValidDeltaTableValue_ReturnsExpectedType(string deltaTableValue, Resolution expectedType)
    {
        // Act
        var actualType = ResolutionMapper.FromDeltaTableValue(deltaTableValue);

        // Assert
        Assert.Equal(expectedType, actualType);
    }

    [Fact]
    public void FromDeltaTableValue_WhenInvalidDeltaTableValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidDeltaTableValue = Guid.NewGuid().ToString();

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => ResolutionMapper.FromDeltaTableValue(invalidDeltaTableValue));
    }

    [Theory]
    [InlineData("P1M", Resolution.Month)]
    [InlineData("P1D", Resolution.Day)]
    [InlineData("PT1H", Resolution.Hour)]
    public void ToDeltaTableValue_WhenValidResolutionValue_ReturnsExpectedString(string expectedDeltaTableValue, Resolution resolution)
    {
        // Act
        var actualDeltaTableValue = ResolutionMapper.ToDeltaTableValue(resolution);

        // Assert
        Assert.Equal(expectedDeltaTableValue, actualDeltaTableValue);
    }

    [Fact]
    public void ToDeltaTableValue_WhenInvalidResolutionValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidResolutionValue = (Resolution)int.MinValue;

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => ResolutionMapper.ToDeltaTableValue(invalidResolutionValue));
    }
}
