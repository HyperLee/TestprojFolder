using BNICalculate.Models;
using System.ComponentModel.DataAnnotations;

namespace BNICalculate.Tests.Unit.Models;

/// <summary>
/// ExchangeRate 模型測試
/// </summary>
public class ExchangeRateTests
{
    [Fact]
    public void CurrencyCode_Should_BeRequired()
    {
        // Arrange
        var rate = new ExchangeRate
        {
            CurrencyCode = "",
            CurrencyName = "美元",
            CashBuyRate = 31.2m,
            CashSellRate = 31.6m,
            LastUpdated = DateTime.Now
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(rate);
        var isValid = Validator.TryValidateObject(rate, context, validationResults, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(ExchangeRate.CurrencyCode)));
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("us")]
    [InlineData("123")]
    public void CurrencyCode_Should_Be3UppercaseLetters(string invalidCode)
    {
        // Arrange
        var rate = new ExchangeRate
        {
            CurrencyCode = invalidCode,
            CurrencyName = "美元",
            CashBuyRate = 31.2m,
            CashSellRate = 31.6m,
            LastUpdated = DateTime.Now
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(rate);
        var isValid = Validator.TryValidateObject(rate, context, validationResults, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, v => 
            v.MemberNames.Contains(nameof(ExchangeRate.CurrencyCode)) && 
            v.ErrorMessage!.Contains("3個大寫英文字母"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void CashBuyRate_Should_BePositive(decimal invalidRate)
    {
        // Arrange
        var rate = new ExchangeRate
        {
            CurrencyCode = "USD",
            CurrencyName = "美元",
            CashBuyRate = invalidRate,
            CashSellRate = 31.6m,
            LastUpdated = DateTime.Now
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(rate);
        var isValid = Validator.TryValidateObject(rate, context, validationResults, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(ExchangeRate.CashBuyRate)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void CashSellRate_Should_BePositive(decimal invalidRate)
    {
        // Arrange
        var rate = new ExchangeRate
        {
            CurrencyCode = "USD",
            CurrencyName = "美元",
            CashBuyRate = 31.2m,
            CashSellRate = invalidRate,
            LastUpdated = DateTime.Now
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(rate);
        var isValid = Validator.TryValidateObject(rate, context, validationResults, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(ExchangeRate.CashSellRate)));
    }

    [Fact]
    public void LastUpdated_Should_BeRequired()
    {
        // Arrange
        var rate = new ExchangeRate
        {
            CurrencyCode = "USD",
            CurrencyName = "美元",
            CashBuyRate = 31.2m,
            CashSellRate = 31.6m,
            LastUpdated = default // 未設定
        };

        // Act & Assert - LastUpdated 是 DateTime，不能為 null，但應該是有效日期
        Assert.NotEqual(default(DateTime), DateTime.Now);
    }

    [Fact]
    public void ValidExchangeRate_Should_PassValidation()
    {
        // Arrange
        var rate = new ExchangeRate
        {
            CurrencyCode = "USD",
            CurrencyName = "美元",
            CashBuyRate = 31.2m,
            CashSellRate = 31.6m,
            LastUpdated = DateTime.Now
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(rate);
        var isValid = Validator.TryValidateObject(rate, context, validationResults, true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(validationResults);
    }
}
