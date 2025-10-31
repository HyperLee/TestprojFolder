using BNICalculate.Models;
using System.ComponentModel.DataAnnotations;

namespace BNICalculate.Tests.Unit.Models;

/// <summary>
/// ExchangeRateData 模型測試
/// </summary>
public class ExchangeRateDataTests
{
    [Fact]
    public void Rates_Should_HaveAtLeastOneItem()
    {
        // Arrange
        var data = new ExchangeRateData
        {
            Rates = new List<ExchangeRate>(), // 空清單
            LastFetchTime = DateTime.Now,
            DataSource = "台灣銀行"
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(data);
        var isValid = Validator.TryValidateObject(data, context, validationResults, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, v => 
            v.MemberNames.Contains(nameof(ExchangeRateData.Rates)) && 
            v.ErrorMessage!.Contains("至少需要一筆"));
    }

    [Fact]
    public void IsStale_Should_ReturnTrue_WhenDataIsOlderThan24Hours()
    {
        // Arrange
        var data = new ExchangeRateData
        {
            Rates = new List<ExchangeRate>
            {
                new ExchangeRate
                {
                    CurrencyCode = "USD",
                    CurrencyName = "美元",
                    CashBuyRate = 31.2m,
                    CashSellRate = 31.6m,
                    LastUpdated = DateTime.Now
                }
            },
            LastFetchTime = DateTime.Now.AddHours(-25), // 25小時前
            DataSource = "台灣銀行"
        };

        // Act
        var isStale = data.IsStale();

        // Assert
        Assert.True(isStale);
    }

    [Fact]
    public void IsStale_Should_ReturnFalse_WhenDataIsWithin24Hours()
    {
        // Arrange
        var data = new ExchangeRateData
        {
            Rates = new List<ExchangeRate>
            {
                new ExchangeRate
                {
                    CurrencyCode = "USD",
                    CurrencyName = "美元",
                    CashBuyRate = 31.2m,
                    CashSellRate = 31.6m,
                    LastUpdated = DateTime.Now
                }
            },
            LastFetchTime = DateTime.Now.AddHours(-23), // 23小時前
            DataSource = "台灣銀行"
        };

        // Act
        var isStale = data.IsStale();

        // Assert
        Assert.False(isStale);
    }

    [Fact]
    public void GetRate_Should_ReturnCorrectRate_WhenCurrencyExists()
    {
        // Arrange
        var usdRate = new ExchangeRate
        {
            CurrencyCode = "USD",
            CurrencyName = "美元",
            CashBuyRate = 31.2m,
            CashSellRate = 31.6m,
            LastUpdated = DateTime.Now
        };

        var data = new ExchangeRateData
        {
            Rates = new List<ExchangeRate> { usdRate },
            LastFetchTime = DateTime.Now,
            DataSource = "台灣銀行"
        };

        // Act
        var rate = data.GetRate("USD");

        // Assert
        Assert.NotNull(rate);
        Assert.Equal("USD", rate.CurrencyCode);
        Assert.Equal(31.2m, rate.CashBuyRate);
    }

    [Fact]
    public void GetRate_Should_ReturnNull_WhenCurrencyDoesNotExist()
    {
        // Arrange
        var data = new ExchangeRateData
        {
            Rates = new List<ExchangeRate>
            {
                new ExchangeRate
                {
                    CurrencyCode = "USD",
                    CurrencyName = "美元",
                    CashBuyRate = 31.2m,
                    CashSellRate = 31.6m,
                    LastUpdated = DateTime.Now
                }
            },
            LastFetchTime = DateTime.Now,
            DataSource = "台灣銀行"
        };

        // Act
        var rate = data.GetRate("JPY");

        // Assert
        Assert.Null(rate);
    }

    [Fact]
    public void GetRate_Should_BeCaseInsensitive()
    {
        // Arrange
        var data = new ExchangeRateData
        {
            Rates = new List<ExchangeRate>
            {
                new ExchangeRate
                {
                    CurrencyCode = "USD",
                    CurrencyName = "美元",
                    CashBuyRate = 31.2m,
                    CashSellRate = 31.6m,
                    LastUpdated = DateTime.Now
                }
            },
            LastFetchTime = DateTime.Now,
            DataSource = "台灣銀行"
        };

        // Act
        var rate = data.GetRate("usd"); // 小寫

        // Assert
        Assert.NotNull(rate);
        Assert.Equal("USD", rate.CurrencyCode);
    }

    [Fact]
    public void ValidExchangeRateData_Should_PassValidation()
    {
        // Arrange
        var data = new ExchangeRateData
        {
            Rates = new List<ExchangeRate>
            {
                new ExchangeRate
                {
                    CurrencyCode = "USD",
                    CurrencyName = "美元",
                    CashBuyRate = 31.2m,
                    CashSellRate = 31.6m,
                    LastUpdated = DateTime.Now
                }
            },
            LastFetchTime = DateTime.Now,
            DataSource = "台灣銀行"
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(data);
        var isValid = Validator.TryValidateObject(data, context, validationResults, true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(validationResults);
    }
}
