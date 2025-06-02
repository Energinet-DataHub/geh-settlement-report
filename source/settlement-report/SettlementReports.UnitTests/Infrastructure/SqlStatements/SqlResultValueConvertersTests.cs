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

using Energinet.DataHub.Reports.Infrastructure.SqlStatements;
using Energinet.DataHub.Reports.Interfaces.CalculationResults.Model;
using Energinet.DataHub.Reports.Interfaces.CalculationResults.Model.EnergyResults;
using NodaTime;
using Xunit;

namespace Energinet.DataHub.Reports.UnitTests.Infrastructure.SqlStatements;

public class SqlResultValueConvertersTests
{
    [Fact]
    public void ToInstant_WhenValueIsNull_ReturnsNull()
    {
        var actual = SqlResultValueConverters.ToInstant(null);
        Assert.Null(actual);
    }

    [Fact]
    public void ToInstant_WhenValueIsValid_ReturnsInstant()
    {
        // Arrange
        var value = "2021-01-01T00:00:00Z";

        // Act
        var actual = SqlResultValueConverters.ToInstant(value);

        // Assert
        Assert.Equal(Instant.FromUtc(2021, 1, 1, 0, 0, 0), actual);
    }

    [Fact]
    public void ToInt_WhenValueIsNull_ReturnsNull()
    {
        var actual = SqlResultValueConverters.ToInt(null);
        Assert.Null(actual);
    }

    [Fact]
    public void ToInt_WhenValueIsValid_ReturnsInt()
    {
        // Arrange
        var value = "123";

        // Act
        var actual = SqlResultValueConverters.ToInt(value);

        // Assert
        Assert.Equal(123, actual);
    }

    [Fact]
    public void ToDecimal_WhenValueIsNull_ReturnsNull()
    {
        var actual = SqlResultValueConverters.ToDecimal(null);
        Assert.Null(actual);
    }

    [Fact]
    public void ToDecimal_WhenValueIsValid_ReturnsDecimal()
    {
        // Arrange
        var value = "1.123";

        // Act
        var actual = SqlResultValueConverters.ToDecimal(value);

        // Assert
        Assert.Equal(1.123m, actual);
    }

    [Fact]
    public void ToDateTimeOffset_WhenValueIsNull_ReturnsNull()
    {
        var actual = SqlResultValueConverters.ToDateTimeOffset(null);
        Assert.Null(actual);
    }

    [Fact]
    public void ToDateTimeOffset_WhenValueIsValid_ReturnsDateTimeOffset()
    {
        // Arrange
        var value = "2021-01-01T00:00:00Z";

        // Act
        var actual = SqlResultValueConverters.ToDateTimeOffset(value);

        // Assert
        Assert.Equal(new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero), actual);
    }

    [Fact]
    public void ToQuantityQualities_WhenValueIsValid_ReturnsQuantityQualities()
    {
        // Arrange
        const string value = "[\"measured\", \"calculated\"]";
        var expected = new List<QuantityQuality> { QuantityQuality.Measured, QuantityQuality.Calculated };

        // Act
        var actual = SqlResultValueConverters.ToQuantityQualities(value);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToTimeSeriesType_WhenValueIsValid_ReturnsTimeSeriesType()
    {
        // Arrange
        var value = "production";

        // Act
        var actual = SqlResultValueConverters.ToTimeSeriesType(value);

        // Assert
        Assert.Equal(TimeSeriesType.Production, actual);
    }
}
