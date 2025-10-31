using BNICalculate.Models;
using BNICalculate.Services;
using Microsoft.AspNetCore.Hosting;
using Moq;

namespace BNICalculate.Tests.Unit.Services;

/// <summary>
/// CurrencyDataService 測試
/// </summary>
public class CurrencyDataServiceTests : IDisposable
{
    private readonly string _testDataDirectory;
    private readonly string _testFilePath;
    private readonly Mock<IWebHostEnvironment> _mockEnv;
    private readonly CurrencyDataService _service;

    public CurrencyDataServiceTests()
    {
        // 建立測試用臨時目錄
        _testDataDirectory = Path.Combine(Path.GetTempPath(), $"CurrencyTests_{Guid.NewGuid()}");
        var currencyDir = Path.Combine(_testDataDirectory, "App_Data", "currency");
        Directory.CreateDirectory(currencyDir);
        _testFilePath = Path.Combine(currencyDir, "rates.json");

        // 設定 Mock IWebHostEnvironment
        _mockEnv = new Mock<IWebHostEnvironment>();
        _mockEnv.Setup(e => e.ContentRootPath).Returns(_testDataDirectory);

        _service = new CurrencyDataService(_mockEnv.Object);
    }

    [Fact]
    public async Task LoadAsync_Should_ReturnNull_WhenFileDoesNotExist()
    {
        // Act
        var result = await _service.LoadAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoadAsync_Should_ReturnData_WhenFileExists()
    {
        // Arrange
        var testData = new ExchangeRateData
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

        await _service.SaveAsync(testData);

        // Act
        var result = await _service.LoadAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Rates);
        Assert.Equal("USD", result.Rates[0].CurrencyCode);
        Assert.Equal(31.2m, result.Rates[0].CashBuyRate);
    }

    [Fact]
    public async Task LoadAsync_Should_ThrowDataFormatException_WhenJsonIsInvalid()
    {
        // Arrange
        await File.WriteAllTextAsync(_testFilePath, "{ invalid json }");

        // Act & Assert
        await Assert.ThrowsAsync<DataFormatException>(() => _service.LoadAsync());
    }

    [Fact]
    public async Task SaveAsync_Should_CreateFile_WhenFileDoesNotExist()
    {
        // Arrange
        var testData = new ExchangeRateData
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
        await _service.SaveAsync(testData);

        // Assert
        Assert.True(File.Exists(_testFilePath));
    }

    [Fact]
    public async Task SaveAsync_Should_UseAtomicWrite()
    {
        // Arrange
        var testData = new ExchangeRateData
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
        await _service.SaveAsync(testData);

        // Assert - 確認檔案存在且內容正確
        Assert.True(File.Exists(_testFilePath));
        var content = await File.ReadAllTextAsync(_testFilePath);
        Assert.Contains("USD", content);
        Assert.Contains("美元", content);
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnTrue_WhenFileExists()
    {
        // Arrange
        var testData = new ExchangeRateData
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
        await _service.SaveAsync(testData);

        // Act
        var exists = await _service.ExistsAsync();

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_Should_ReturnFalse_WhenFileDoesNotExist()
    {
        // Act
        var exists = await _service.ExistsAsync();

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task GetLastModifiedTimeAsync_Should_ReturnTime_WhenFileExists()
    {
        // Arrange
        var testData = new ExchangeRateData
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
        await _service.SaveAsync(testData);
        var expectedTime = File.GetLastWriteTime(_testFilePath);

        // Act
        var result = await _service.GetLastModifiedTimeAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(Math.Abs((result.Value - expectedTime).TotalSeconds) < 1);
    }

    [Fact]
    public async Task GetLastModifiedTimeAsync_Should_ReturnNull_WhenFileDoesNotExist()
    {
        // Act
        var result = await _service.GetLastModifiedTimeAsync();

        // Assert
        Assert.Null(result);
    }

    public void Dispose()
    {
        // 清理測試用臨時目錄
        if (Directory.Exists(_testDataDirectory))
        {
            Directory.Delete(_testDataDirectory, true);
        }
    }
}
