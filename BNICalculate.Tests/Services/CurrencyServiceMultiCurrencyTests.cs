using BNICalculate.Models;
using BNICalculate.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace BNICalculate.Tests.Services;

/// <summary>
/// 多貨幣支援測試
/// </summary>
public class CurrencyServiceMultiCurrencyTests
{
    private readonly Mock<ICurrencyDataService> _mockDataService;
    private readonly IMemoryCache _cache;
    private readonly CurrencyService _service;

    public CurrencyServiceMultiCurrencyTests()
    {
        _mockDataService = new Mock<ICurrencyDataService>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockLogger = new Mock<ILogger<CurrencyService>>();
        _service = new CurrencyService(
            _mockDataService.Object, 
            _cache,
            mockHttpClientFactory.Object,
            mockLogger.Object
        );
    }

    [Theory]
    [InlineData("USD", 1000, 30.5)]  // 美元
    [InlineData("JPY", 1000, 0.21)]  // 日圓
    [InlineData("CNY", 1000, 4.3)]   // 人民幣
    [InlineData("EUR", 1000, 33.8)]  // 歐元
    [InlineData("GBP", 1000, 38.8)]  // 英鎊
    [InlineData("HKD", 1000, 3.9)]   // 港幣
    [InlineData("AUD", 1000, 20.5)]  // 澳幣
    public async Task CalculateTwdToForeignAsync_AllSupportedCurrencies_ShouldCalculateCorrectly(
        string currencyCode, decimal twdAmount, decimal expectedRate)
    {
        // Arrange
        var ratesData = CreateTestRatesDataWithAllCurrencies();
        _mockDataService.Setup(s => s.LoadAsync()).ReturnsAsync(ratesData);

        // Act
        var result = await _service.CalculateTwdToForeignAsync(twdAmount, currencyCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TWD", result.SourceCurrency);
        Assert.Equal(currencyCode, result.TargetCurrency);
        Assert.Equal(twdAmount, result.SourceAmount);
        Assert.Equal(expectedRate, result.ExchangeRate);
        
        // 驗證計算結果
        var expectedAmount = twdAmount / expectedRate;
        Assert.Equal(expectedAmount, result.TargetAmount, 2);
    }

    [Theory]
    [InlineData("USD", 100, 31.0)]   // 美元
    [InlineData("JPY", 1000, 0.22)]  // 日圓
    [InlineData("CNY", 100, 4.4)]    // 人民幣
    [InlineData("EUR", 100, 34.5)]   // 歐元
    [InlineData("GBP", 100, 39.5)]   // 英鎊
    [InlineData("HKD", 100, 4.0)]    // 港幣
    [InlineData("AUD", 100, 21.0)]   // 澳幣
    public async Task CalculateForeignToTwdAsync_AllSupportedCurrencies_ShouldCalculateCorrectly(
        string currencyCode, decimal foreignAmount, decimal expectedRate)
    {
        // Arrange
        var ratesData = CreateTestRatesDataWithAllCurrencies();
        _mockDataService.Setup(s => s.LoadAsync()).ReturnsAsync(ratesData);

        // Act
        var result = await _service.CalculateForeignToTwdAsync(foreignAmount, currencyCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(currencyCode, result.SourceCurrency);
        Assert.Equal("TWD", result.TargetCurrency);
        Assert.Equal(foreignAmount, result.SourceAmount);
        Assert.Equal(expectedRate, result.ExchangeRate);
        
        // 驗證計算結果
        var expectedAmount = foreignAmount * expectedRate;
        Assert.Equal(expectedAmount, result.TargetAmount, 2);
    }

    [Fact]
    public async Task GetRatesAsync_ShouldReturnAllSevenCurrencies()
    {
        // Arrange
        var ratesData = CreateTestRatesDataWithAllCurrencies();
        _mockDataService.Setup(s => s.LoadAsync()).ReturnsAsync(ratesData);

        // Act
        var result = await _service.GetRatesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(7, result.Rates.Count);
        
        var currencyCodes = result.Rates.Select(r => r.CurrencyCode).ToList();
        Assert.Contains("USD", currencyCodes);
        Assert.Contains("JPY", currencyCodes);
        Assert.Contains("CNY", currencyCodes);
        Assert.Contains("EUR", currencyCodes);
        Assert.Contains("GBP", currencyCodes);
        Assert.Contains("HKD", currencyCodes);
        Assert.Contains("AUD", currencyCodes);
    }

    [Fact]
    public async Task CalculateTwdToForeignAsync_UnsupportedCurrency_ShouldThrowException()
    {
        // Arrange
        var ratesData = CreateTestRatesDataWithAllCurrencies();
        _mockDataService.Setup(s => s.LoadAsync()).ReturnsAsync(ratesData);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.CalculateTwdToForeignAsync(1000, "XXX")
        );
    }

    [Fact]
    public async Task CalculateForeignToTwdAsync_UnsupportedCurrency_ShouldThrowException()
    {
        // Arrange
        var ratesData = CreateTestRatesDataWithAllCurrencies();
        _mockDataService.Setup(s => s.LoadAsync()).ReturnsAsync(ratesData);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.CalculateForeignToTwdAsync(100, "XXX")
        );
    }

    private static ExchangeRateData CreateTestRatesDataWithAllCurrencies()
    {
        return new ExchangeRateData
        {
            DataSource = "臺灣銀行",
            LastFetchTime = DateTime.Now,
            Rates = new List<ExchangeRate>
            {
                new ExchangeRate
                {
                    CurrencyCode = "USD",
                    CurrencyName = "美元",
                    CashBuyRate = 30.5m,
                    CashSellRate = 31.0m,
                    LastUpdated = DateTime.Now
                },
                new ExchangeRate
                {
                    CurrencyCode = "JPY",
                    CurrencyName = "日圓",
                    CashBuyRate = 0.20m,
                    CashSellRate = 0.22m,
                    LastUpdated = DateTime.Now
                },
                new ExchangeRate
                {
                    CurrencyCode = "CNY",
                    CurrencyName = "人民幣",
                    CashBuyRate = 4.2m,
                    CashSellRate = 4.4m,
                    LastUpdated = DateTime.Now
                },
                new ExchangeRate
                {
                    CurrencyCode = "EUR",
                    CurrencyName = "歐元",
                    CashBuyRate = 33.5m,
                    CashSellRate = 34.5m,
                    LastUpdated = DateTime.Now
                },
                new ExchangeRate
                {
                    CurrencyCode = "GBP",
                    CurrencyName = "英鎊",
                    CashBuyRate = 38.5m,
                    CashSellRate = 39.5m,
                    LastUpdated = DateTime.Now
                },
                new ExchangeRate
                {
                    CurrencyCode = "HKD",
                    CurrencyName = "港幣",
                    CashBuyRate = 3.8m,
                    CashSellRate = 4.0m,
                    LastUpdated = DateTime.Now
                },
                new ExchangeRate
                {
                    CurrencyCode = "AUD",
                    CurrencyName = "澳幣",
                    CashBuyRate = 20.0m,
                    CashSellRate = 21.0m,
                    LastUpdated = DateTime.Now
                }
            }
        };
    }
}
