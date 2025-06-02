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

using Energinet.DataHub.Reports.Infrastructure.SqlStatements.Mappers.WholesaleResult;
using Energinet.DataHub.Reports.Interfaces.CalculationResults.Model.WholesaleResults;
using Xunit;

namespace Energinet.DataHub.Reports.UnitTests.Infrastructure.SqlStatements.Mappers.WholesaleResult;

public class AmountTypeMapperTests
{
    [Theory]
    [InlineData("amount_per_charge", AmountType.AmountPerCharge)]
    [InlineData("monthly_amount_per_charge", AmountType.MonthlyAmountPerCharge)]
    [InlineData("total_monthly_amount", AmountType.TotalMonthlyAmount)]
    public void FromDeltaTableValue_WhenValidDeltaTableValue_ReturnsExpectedType(string deltaTableValue, AmountType expectedType)
    {
        // Act
        var actualType = AmountTypeMapper.FromDeltaTableValue(deltaTableValue);

        // Assert
        Assert.Equal(expectedType, actualType);
    }

    [Fact]
    public void FromDeltaTableValue_WhenInvalidDeltaTableValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidDeltaTableValue = Guid.NewGuid().ToString();

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => AmountTypeMapper.FromDeltaTableValue(invalidDeltaTableValue));
    }

    [Theory]
    [InlineData("amount_per_charge", AmountType.AmountPerCharge)]
    [InlineData("monthly_amount_per_charge", AmountType.MonthlyAmountPerCharge)]
    [InlineData("total_monthly_amount", AmountType.TotalMonthlyAmount)]
    public void ToDeltaTableValue_WhenValidAmountTypeValue_ReturnsExpectedString(string expectedDeltaTableValue, AmountType amountType)
    {
        // Act
        var actualDeltaTableValue = AmountTypeMapper.ToDeltaTableValue(amountType);

        // Assert
        Assert.Equal(expectedDeltaTableValue, actualDeltaTableValue);
    }

    [Fact]
    public void ToDeltaTableValue_WhenInvalidAmountTypeValue_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var invalidValue = (AmountType)int.MinValue;

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => AmountTypeMapper.ToDeltaTableValue(invalidValue));
    }
}
