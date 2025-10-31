using BNICalculate.Models;
using BNICalculate.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;

namespace BNICalculate.Tests.Services;

public class CurrencyServiceRateUpdateTests
{
    private readonly Mock<ICurrencyDataService> _mockDataService;
    private readonly Mock<IMemoryCache> _mockCache;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILogger<CurrencyService>> _mockLogger;
    private readonly CurrencyService _service;

    public CurrencyServiceRateUpdateTests()
    {
        _mockDataService = new Mock<ICurrencyDataService>();
        _mockCache = new Mock<IMemoryCache>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockLogger = new Mock<ILogger<CurrencyService>>();

        _service = new CurrencyService(
            _mockDataService.Object,
            _mockCache.Object,
            _mockHttpClientFactory.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task FetchAndUpdateRatesAsync_Success_ShouldUpdateDataAndClearCache()
    {
        // Arrange
        var csvContent = CreateValidCsvContent();
        var mockHttpMessageHandler = CreateMockHttpMessageHandler(csvContent, HttpStatusCode.OK);
        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://rate.bot.com.tw")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("TaiwanBankApi"))
            .Returns(httpClient);

        object? cacheValue = null;
        _mockCache
            .Setup(c => c.TryGetValue(It.IsAny<object>(), out cacheValue))
            .Returns(false);

        _mockCache
            .Setup(c => c.Remove(It.IsAny<object>()))
            .Verifiable();

        _mockDataService
            .Setup(s => s.SaveAsync(It.IsAny<ExchangeRateData>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        // Act
        await _service.FetchAndUpdateRatesAsync();

        // Assert
        _mockDataService.Verify(s => s.SaveAsync(It.Is<ExchangeRateData>(data =>
            data.Rates.Count == 7 &&
            data.Rates.Any(r => r.CurrencyCode == "USD")
        )), Times.Once);

        _mockCache.Verify(c => c.Remove("exchange_rates"), Times.Once);
    }

    [Fact]
    public async Task FetchAndUpdateRatesAsync_HttpRequestException_ShouldThrowExternalServiceException()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://rate.bot.com.tw")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("TaiwanBankApi"))
            .Returns(httpClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ExternalServiceException>(
            async () => await _service.FetchAndUpdateRatesAsync()
        );

        Assert.Contains("取得匯率資料失敗", exception.Message);
    }

    [Fact]
    public async Task FetchAndUpdateRatesAsync_NonSuccessStatusCode_ShouldThrowExternalServiceException()
    {
        // Arrange
        var mockHttpMessageHandler = CreateMockHttpMessageHandler("", HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://rate.bot.com.tw")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("TaiwanBankApi"))
            .Returns(httpClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ExternalServiceException>(
            async () => await _service.FetchAndUpdateRatesAsync()
        );

        Assert.Contains("取得匯率資料失敗", exception.Message);
    }

    [Fact]
    public async Task FetchAndUpdateRatesAsync_InvalidCsvFormat_ShouldThrowDataFormatException()
    {
        // Arrange
        var invalidCsv = "Invalid,CSV,Format\n1,2,3";
        var mockHttpMessageHandler = CreateMockHttpMessageHandler(invalidCsv, HttpStatusCode.OK);
        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://rate.bot.com.tw")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("TaiwanBankApi"))
            .Returns(httpClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DataFormatException>(
            async () => await _service.FetchAndUpdateRatesAsync()
        );

        Assert.Contains("CSV 格式錯誤", exception.Message);
    }

    [Fact]
    public async Task FetchAndUpdateRatesAsync_NoValidRates_ShouldThrowDataFormatException()
    {
        // Arrange
        var csvContent = "幣別,幣別名稱,本行現金買入,本行現金賣出,即期買入,即期賣出\n" +
                        "XXX,Unknown,-,-,-,-\n";
        var mockHttpMessageHandler = CreateMockHttpMessageHandler(csvContent, HttpStatusCode.OK);
        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://rate.bot.com.tw")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("TaiwanBankApi"))
            .Returns(httpClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DataFormatException>(
            async () => await _service.FetchAndUpdateRatesAsync()
        );

        Assert.Contains("CSV 格式錯誤", exception.Message);
    }

    [Fact]
    public async Task FetchAndUpdateRatesAsync_TaskCanceled_ShouldThrowExternalServiceException()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new TaskCanceledException("Timeout"));

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://rate.bot.com.tw")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("TaiwanBankApi"))
            .Returns(httpClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ExternalServiceException>(
            async () => await _service.FetchAndUpdateRatesAsync()
        );

        Assert.Contains("取得匯率資料失敗", exception.Message);
    }

    [Fact]
    public async Task FetchAndUpdateRatesAsync_FiltersUnsupportedCurrencies()
    {
        // Arrange
        var csvContent = "幣別,幣別名稱,本行現金買入,本行現金賣出,即期買入,即期賣出\n" +
                        "USD,美金,30.5,31.0,30.7,30.8\n" +
                        "ZZZ,未支援貨幣,1.0,1.5,1.2,1.3\n" +
                        "JPY,日圓,0.20,0.22,0.21,0.21\n";

        var mockHttpMessageHandler = CreateMockHttpMessageHandler(csvContent, HttpStatusCode.OK);
        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://rate.bot.com.tw")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("TaiwanBankApi"))
            .Returns(httpClient);

        ExchangeRateData? savedData = null;
        _mockDataService
            .Setup(s => s.SaveAsync(It.IsAny<ExchangeRateData>()))
            .Callback<ExchangeRateData>(data => savedData = data)
            .Returns(Task.CompletedTask);

        // Act
        await _service.FetchAndUpdateRatesAsync();

        // Assert
        Assert.NotNull(savedData);
        Assert.Equal(2, savedData.Rates.Count);
        Assert.Contains(savedData.Rates, r => r.CurrencyCode == "USD");
        Assert.Contains(savedData.Rates, r => r.CurrencyCode == "JPY");
        Assert.DoesNotContain(savedData.Rates, r => r.CurrencyCode == "ZZZ");
    }

    [Fact]
    public async Task FetchAndUpdateRatesAsync_SkipsInvalidRates()
    {
        // Arrange
        var csvContent = "幣別,幣別名稱,本行現金買入,本行現金賣出,即期買入,即期賣出\n" +
                        "USD,美金,30.5,31.0,30.7,30.8\n" +
                        "EUR,歐元,-,-,35.0,35.5\n" +  // 現金匯率無效
                        "JPY,日圓,0.20,0.22,0.21,0.21\n";

        var mockHttpMessageHandler = CreateMockHttpMessageHandler(csvContent, HttpStatusCode.OK);
        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://rate.bot.com.tw")
        };

        _mockHttpClientFactory
            .Setup(f => f.CreateClient("TaiwanBankApi"))
            .Returns(httpClient);

        ExchangeRateData? savedData = null;
        _mockDataService
            .Setup(s => s.SaveAsync(It.IsAny<ExchangeRateData>()))
            .Callback<ExchangeRateData>(data => savedData = data)
            .Returns(Task.CompletedTask);

        // Act
        await _service.FetchAndUpdateRatesAsync();

        // Assert
        Assert.NotNull(savedData);
        Assert.Equal(2, savedData.Rates.Count);
        Assert.Contains(savedData.Rates, r => r.CurrencyCode == "USD");
        Assert.Contains(savedData.Rates, r => r.CurrencyCode == "JPY");
        Assert.DoesNotContain(savedData.Rates, r => r.CurrencyCode == "EUR");
    }

    private static string CreateValidCsvContent()
    {
        return "幣別,幣別名稱,本行現金買入,本行現金賣出,即期買入,即期賣出\n" +
               "USD,美金,30.5,31.0,30.7,30.8\n" +
               "JPY,日圓,0.20,0.22,0.21,0.21\n" +
               "CNY,人民幣,4.2,4.4,4.3,4.35\n" +
               "EUR,歐元,33.5,34.5,33.8,34.0\n" +
               "GBP,英鎊,38.5,39.5,38.8,39.0\n" +
               "HKD,港幣,3.8,4.0,3.9,3.95\n" +
               "AUD,澳幣,20.0,21.0,20.5,20.8\n";
    }

    private static Mock<HttpMessageHandler> CreateMockHttpMessageHandler(
        string content,
        HttpStatusCode statusCode)
    {
        var mockHandler = new Mock<HttpMessageHandler>();

        // 將內容轉換為 Big5 編碼
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var big5Encoding = Encoding.GetEncoding("Big5");
        var contentBytes = big5Encoding.GetBytes(content);

        var response = new HttpResponseMessage(statusCode)
        {
            Content = new ByteArrayContent(contentBytes)
        };

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        return mockHandler;
    }
}
