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

using Energinet.DataHub.SettlementReport.Common.Interfaces.Models;
using Energinet.DataHub.SettlementReport.Infrastructure.SqlStatements.Mappers.WholesaleResult;
using Xunit;

namespace Energinet.DataHub.SettlementReport.UnitTests.Infrastructure.SqlStatements.Mappers.WholesaleResult;

public class QuantityUnitMapperTests
{
    [Theory]
    [InlineData("kWh", QuantityUnit.Kwh)]
    [InlineData("pcs", QuantityUnit.Pieces)]
    public void FromDeltaTableValue_WhenValidDeltaTableValue_ReturnsExpectedType(string deltaTableValue, QuantityUnit expectedType)
    {
        // Act
        var actualType = QuantityUnitMapper.FromDeltaTableValue(deltaTableValue);

        // Assert
        Assert.Equal(expectedType, actualType);
    }

    [Fact]
    public void FromDeltaTableValue_WhenInvalidDeltaTableValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidDeltaTableValue = Guid.NewGuid().ToString();

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => QuantityUnitMapper.FromDeltaTableValue(invalidDeltaTableValue));
    }
}
