using BNICalculate.Models;
using BNICalculate.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace BNICalculate.Tests.Unit.Services;

/// <summary>
/// CurrencyService 測試
/// </summary>
public class CurrencyServiceTests
{
    private readonly Mock<ICurrencyDataService> _mockDataService;
    private readonly IMemoryCache _cache;
    private readonly CurrencyService _service;

    public CurrencyServiceTests()
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

    [Fact]
    public async Task CalculateTwdToForeignAsync_Should_CalculateCorrectly()
    {
        // Arrange
        var ratesData = CreateTestRatesData();
        _mockDataService.Setup(s => s.LoadAsync()).ReturnsAsync(ratesData);

        // Act
        var result = await _service.CalculateTwdToForeignAsync(1000, "USD");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1000, result.SourceAmount);
        Assert.Equal("TWD", result.SourceCurrency);
        Assert.Equal("USD", result.TargetCurrency);
        Assert.Equal(0.031646m, result.ExchangeRate); // 1 / 31.6
        // 1000 / 31.6 = 31.645570 (四捨五入到6位小數)
        Assert.Equal(31.645570m, result.TargetAmount, 6);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task CalculateTwdToForeignAsync_Should_ThrowArgumentException_WhenAmountInvalid(decimal invalidAmount)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.CalculateTwdToForeignAsync(invalidAmount, "USD"));
    }

    [Fact]
    public async Task CalculateTwdToForeignAsync_Should_ThrowArgumentException_WhenCurrencyNotSupported()
    {
        // Arrange
        var ratesData = CreateTestRatesData();
        _mockDataService.Setup(s => s.LoadAsync()).ReturnsAsync(ratesData);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.CalculateTwdToForeignAsync(1000, "XXX"));
    }

    [Fact]
    public async Task CalculateTwdToForeignAsync_Should_ThrowInvalidOperationException_WhenNoData()
    {
        // Arrange
        _mockDataService.Setup(s => s.LoadAsync()).ReturnsAsync((ExchangeRateData?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.CalculateTwdToForeignAsync(1000, "USD"));
    }

    [Theory]
    [InlineData(1000, 31.645570)]
    [InlineData(5000, 158.227848)]
    [InlineData(10000, 316.455696)]
    public async Task CalculateTwdToForeignAsync_Should_PreserveDecimalPrecision(decimal twdAmount, decimal expectedUsd)
    {
        // Arrange
        var ratesData = CreateTestRatesData();
        _mockDataService.Setup(s => s.LoadAsync()).ReturnsAsync(ratesData);

        // Act
        var result = await _service.CalculateTwdToForeignAsync(twdAmount, "USD");

        // Assert
        Assert.Equal(expectedUsd, result.TargetAmount, 6);
    }

    [Fact]
    public async Task GetRatesAsync_Should_ReturnCachedData_WhenCacheHit()
    {
        // Arrange
        var ratesData = CreateTestRatesData();
        _mockDataService.Setup(s => s.LoadAsync()).ReturnsAsync(ratesData);

        // 第一次呼叫，載入資料
        await _service.GetRatesAsync();

        // Act - 第二次呼叫，應該使用快取
        var result = await _service.GetRatesAsync();

        // Assert
        Assert.NotNull(result);
        // 驗證只呼叫一次 LoadAsync（使用快取）
        _mockDataService.Verify(s => s.LoadAsync(), Times.Once);
    }

    [Fact]
    public async Task GetRatesAsync_Should_LoadFromFile_WhenCacheMiss()
    {
        // Arrange
        var ratesData = CreateTestRatesData();
        _mockDataService.Setup(s => s.LoadAsync()).ReturnsAsync(ratesData);

        // Act
        var result = await _service.GetRatesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Rates);
        _mockDataService.Verify(s => s.LoadAsync(), Times.Once);
    }

    [Fact]
    public async Task GetRatesAsync_Should_ReturnNull_WhenNoData()
    {
        // Arrange
        _mockDataService.Setup(s => s.LoadAsync()).ReturnsAsync((ExchangeRateData?)null);

        // Act
        var result = await _service.GetRatesAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task IsDataStaleAsync_Should_ReturnTrue_WhenNoData()
    {
        // Arrange
        _mockDataService.Setup(s => s.LoadAsync()).ReturnsAsync((ExchangeRateData?)null);

        // Act
        var isStale = await _service.IsDataStaleAsync();

        // Assert
        Assert.True(isStale);
    }

    [Fact]
    public async Task IsDataStaleAsync_Should_ReturnFalse_WhenDataIsFresh()
    {
        // Arrange
        var ratesData = CreateTestRatesData();
        ratesData.LastFetchTime = DateTime.Now.AddHours(-1); // 1小時前
        _mockDataService.Setup(s => s.LoadAsync()).ReturnsAsync(ratesData);

        // Act
        var isStale = await _service.IsDataStaleAsync();

        // Assert
        Assert.False(isStale);
    }

    [Fact]
    public async Task IsDataStaleAsync_Should_ReturnTrue_WhenDataIsOld()
    {
        // Arrange
        var ratesData = CreateTestRatesData();
        ratesData.LastFetchTime = DateTime.Now.AddHours(-25); // 25小時前
        _mockDataService.Setup(s => s.LoadAsync()).ReturnsAsync(ratesData);

        // Act
        var isStale = await _service.IsDataStaleAsync();

        // Assert
        Assert.True(isStale);
    }

    private ExchangeRateData CreateTestRatesData()
    {
        return new ExchangeRateData
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
    }
}
