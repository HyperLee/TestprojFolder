using BNICalculate.Models;

namespace BNICalculate.Tests.Unit.Models;

/// <summary>
/// CalculationResult 模型測試
/// </summary>
public class CalculationResultTests
{
    [Fact]
    public void GetFormattedResult_Should_ReturnCorrectFormat_ForTwdToForeign()
    {
        // Arrange
        var result = new CalculationResult
        {
            SourceAmount = 1000,
            SourceCurrency = "TWD",
            TargetAmount = 31.847134m,
            TargetCurrency = "USD",
            ExchangeRate = 0.031847m,
            CalculatedAt = DateTime.Now
        };

        // Act
        var formatted = result.GetFormattedResult();

        // Assert
        Assert.Contains("1,000", formatted);
        Assert.Contains("TWD", formatted);
        Assert.Contains("31.847134", formatted);
        Assert.Contains("USD", formatted);
    }

    [Fact]
    public void GetFormattedResult_Should_ReturnCorrectFormat_ForForeignToTwd()
    {
        // Arrange
        var result = new CalculationResult
        {
            SourceAmount = 100,
            SourceCurrency = "USD",
            TargetAmount = 3120.000000m,
            TargetCurrency = "TWD",
            ExchangeRate = 31.2m,
            CalculatedAt = DateTime.Now
        };

        // Act
        var formatted = result.GetFormattedResult();

        // Assert
        Assert.Contains("100", formatted);
        Assert.Contains("USD", formatted);
        Assert.Contains("3,120.000000", formatted);
        Assert.Contains("TWD", formatted);
    }

    [Fact]
    public void TargetAmount_Should_PreserveDecimalPrecision()
    {
        // Arrange
        var result = new CalculationResult
        {
            SourceAmount = 1000,
            SourceCurrency = "TWD",
            TargetAmount = 31.847134m, // 6位小數
            TargetCurrency = "USD",
            ExchangeRate = 0.031847m,
            CalculatedAt = DateTime.Now
        };

        // Act & Assert
        Assert.Equal(31.847134m, result.TargetAmount);
        Assert.Equal(6, BitConverter.GetBytes(decimal.GetBits(result.TargetAmount)[3])[2]);
    }

    [Theory]
    [InlineData(1000, 31.847134)]
    [InlineData(5000, 159.235670)]
    [InlineData(10000, 318.471340)]
    public void TargetAmount_Should_SupportVarious6DecimalValues(decimal sourceAmount, decimal expectedTarget)
    {
        // Arrange
        var result = new CalculationResult
        {
            SourceAmount = sourceAmount,
            SourceCurrency = "TWD",
            TargetAmount = expectedTarget,
            TargetCurrency = "USD",
            ExchangeRate = 0.031847m,
            CalculatedAt = DateTime.Now
        };

        // Act
        var formatted = result.GetFormattedResult();

        // Assert
        Assert.Contains(expectedTarget.ToString("N6"), formatted);
    }
}
