# Quick Start: 台幣與外幣匯率計算器

**日期**: 2025年11月1日  
**目的**: 快速開始開發指南

## 前置需求

### 開發環境

- .NET 8.0 SDK 或更高版本
- Visual Studio Code 或 Visual Studio 2022+
- Git

### 專案依賴

現有專案 `BNICalculate` 已包含以下套件：

- ASP.NET Core 8.0 Razor Pages
- Bootstrap 5
- jQuery 與 jQuery Validation
- xUnit 測試框架

### 新增套件需求

```bash
# 在 BNICalculate 專案目錄執行
dotnet add package CsvHelper --version 30.0.1
dotnet add package System.Text.Encoding.CodePages --version 8.0.0
dotnet add package Serilog.AspNetCore --version 8.0.0
dotnet add package Serilog.Sinks.Console --version 5.0.1
dotnet add package Serilog.Sinks.File --version 5.0.0
```

---

## 專案結構概覽

```text
BNICalculate/
├── Pages/
│   ├── CurrencyConverter.cshtml          # 新增頁面
│   └── CurrencyConverter.cshtml.cs       # 新增 PageModel
├── Services/
│   ├── ICurrencyService.cs               # 新增介面
│   ├── CurrencyService.cs                # 新增實作
│   ├── ICurrencyDataService.cs           # 新增介面
│   └── CurrencyDataService.cs            # 新增實作
├── Models/
│   ├── ExchangeRate.cs                   # 新增模型
│   ├── ExchangeRateData.cs               # 新增模型
│   ├── Currency.cs                       # 新增列舉
│   ├── CalculationRequest.cs             # 新增模型
│   └── CalculationResult.cs              # 新增模型
├── App_Data/currency/
│   └── rates.json                        # 自動建立
└── wwwroot/
    ├── css/currency-converter.css        # 新增樣式
    └── js/currency-converter.js          # 新增腳本
```

---

## 開發步驟（TDD 流程）

### Phase 1: 建立資料模型與測試（Red）

#### 1.1 建立測試專案結構

```bash
# 在 BNICalculate.Tests 專案目錄
mkdir -p Unit/Services
mkdir -p Integration/Pages
```

#### 1.2 建立 ExchangeRate 模型測試

**檔案**: `BNICalculate.Tests/Unit/Models/ExchangeRateTests.cs`

```csharp
public class ExchangeRateTests
{
    [Fact]
    public void ExchangeRate_ValidData_PassesValidation()
    {
        // Arrange
        var rate = new ExchangeRate
        {
            CurrencyCode = "USD",
            CurrencyName = "美元",
            CashBuyRate = 31.200m,
            CashSellRate = 31.600m,
            LastUpdated = DateTime.Now
        };

        // Act
        var context = new ValidationContext(rate);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(rate, context, results, true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData("US")]  // 太短
    [InlineData("USDA")]  // 太長
    [InlineData("usd")]  // 小寫
    public void ExchangeRate_InvalidCurrencyCode_FailsValidation(string code)
    {
        // Arrange
        var rate = new ExchangeRate { CurrencyCode = code };

        // Act & Assert
        var context = new ValidationContext(rate);
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(rate, context, results, true);
        
        Assert.False(isValid);
    }
}
```

**執行測試**:

```bash
dotnet test --filter "FullyQualifiedName~ExchangeRateTests"
```

**預期結果**: ❌ 測試失敗（模型尚未建立）

#### 1.3 實作 ExchangeRate 模型（Green）

**檔案**: `BNICalculate/Models/ExchangeRate.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace BNICalculate.Models;

/// <summary>
/// 代表特定貨幣的匯率資訊
/// </summary>
public class ExchangeRate
{
    /// <summary>
    /// 貨幣代碼（如 USD, JPY）
    /// </summary>
    [Required]
    [StringLength(3, MinimumLength = 3)]
    [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "貨幣代碼必須為3個大寫英文字母")]
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// 貨幣名稱（如美元、日圓）
    /// </summary>
    [Required]
    [StringLength(50)]
    public string CurrencyName { get; set; } = string.Empty;

    /// <summary>
    /// 現金買入匯率
    /// </summary>
    [Required]
    [Range(0.000001, 999999)]
    public decimal CashBuyRate { get; set; }

    /// <summary>
    /// 現金賣出匯率
    /// </summary>
    [Required]
    [Range(0.000001, 999999)]
    public decimal CashSellRate { get; set; }

    /// <summary>
    /// 最後更新時間
    /// </summary>
    [Required]
    public DateTime LastUpdated { get; set; }
}
```

**執行測試**:

```bash
dotnet test --filter "FullyQualifiedName~ExchangeRateTests"
```

**預期結果**: ✅ 測試通過

#### 1.4 重複此流程建立其他模型

按照相同的 TDD 流程建立：

- `ExchangeRateData`
- `Currency`
- `CalculationRequest`
- `CalculationResult`

---

### Phase 2: 建立資料存取服務（Red → Green → Refactor）

#### 2.1 建立 CurrencyDataService 測試

**檔案**: `BNICalculate.Tests/Unit/Services/CurrencyDataServiceTests.cs`

```csharp
public class CurrencyDataServiceTests : IDisposable
{
    private readonly string _testFilePath;
    private readonly CurrencyDataService _service;

    public CurrencyDataServiceTests()
    {
        _testFilePath = Path.Combine(Path.GetTempPath(), "test_rates.json");
        
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());
        
        _service = new CurrencyDataService(mockEnv.Object);
    }

    [Fact]
    public async Task SaveAsync_ValidData_CreatesFile()
    {
        // Arrange
        var data = new ExchangeRateData
        {
            LastFetchTime = DateTime.Now,
            DataSource = "台灣銀行",
            Rates = new List<ExchangeRate>
            {
                new ExchangeRate
                {
                    CurrencyCode = "USD",
                    CurrencyName = "美元",
                    CashBuyRate = 31.200m,
                    CashSellRate = 31.600m,
                    LastUpdated = DateTime.Now
                }
            }
        };

        // Act
        await _service.SaveAsync(data);

        // Assert
        Assert.True(await _service.ExistsAsync());
    }

    [Fact]
    public async Task LoadAsync_ExistingFile_ReturnsData()
    {
        // Arrange
        var originalData = new ExchangeRateData
        {
            LastFetchTime = DateTime.Now,
            Rates = new List<ExchangeRate>
            {
                new ExchangeRate { CurrencyCode = "USD", CashBuyRate = 31.2m, CashSellRate = 31.6m }
            }
        };
        await _service.SaveAsync(originalData);

        // Act
        var loadedData = await _service.LoadAsync();

        // Assert
        Assert.NotNull(loadedData);
        Assert.Single(loadedData.Rates);
        Assert.Equal("USD", loadedData.Rates[0].CurrencyCode);
    }

    public void Dispose()
    {
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }
}
```

**執行測試**: ❌ 失敗（服務未實作）

#### 2.2 實作 CurrencyDataService

**檔案**: `BNICalculate/Services/CurrencyDataService.cs`

```csharp
using System.Text.Json;
using System.Text.Encodings.Web;
using BNICalculate.Models;
using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace BNICalculate.Services;

/// <summary>
/// 匯率資料存取服務實作
/// </summary>
public class CurrencyDataService : ICurrencyDataService
{
    private readonly ILogger _logger;
    private readonly string _dataFilePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public CurrencyDataService(
        IWebHostEnvironment environment)
    {
        _logger = Log.ForContext<CurrencyDataService>();
        _dataFilePath = Path.Combine(
            environment.ContentRootPath,
            "App_Data",
            "currency",
            "rates.json");

        // 確保目錄存在
        var directory = Path.GetDirectoryName(_dataFilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public async Task<ExchangeRateData?> LoadAsync()
    {
        if (!File.Exists(_dataFilePath))
        {
            _logger.LogWarning("匯率資料檔案不存在: {FilePath}", _dataFilePath);
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_dataFilePath);
            var data = JsonSerializer.Deserialize<ExchangeRateData>(json, JsonOptions);
            _logger.LogInformation("成功載入匯率資料，共 {Count} 種貨幣", data?.Rates.Count ?? 0);
            return data;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON 解析失敗");
            throw new DataFormatException("匯率資料格式異常", ex);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "檔案讀取失敗");
            throw;
        }
    }

    public async Task SaveAsync(ExchangeRateData data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, JsonOptions);
            
            // 原子寫入：先寫暫存檔，再重新命名
            var tempPath = _dataFilePath + ".tmp";
            await File.WriteAllTextAsync(tempPath, json);
            File.Move(tempPath, _dataFilePath, overwrite: true);
            
            _logger.LogInformation("成功儲存匯率資料，共 {Count} 種貨幣", data.Rates.Count);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "檔案寫入失敗");
            throw;
        }
    }

    public Task<bool> ExistsAsync()
    {
        return Task.FromResult(File.Exists(_dataFilePath));
    }

    public Task<DateTime?> GetLastModifiedTimeAsync()
    {
        if (!File.Exists(_dataFilePath))
        {
            return Task.FromResult<DateTime?>(null);
        }

        var lastModified = File.GetLastWriteTime(_dataFilePath);
        return Task.FromResult<DateTime?>(lastModified);
    }
}
```

**執行測試**: ✅ 通過

---

### Phase 3: 建立匯率計算服務

#### 3.1 建立 CurrencyService 測試

**檔案**: `BNICalculate.Tests/Unit/Services/CurrencyServiceTests.cs`

```csharp
public class CurrencyServiceTests
{
    [Fact]
    public async Task CalculateTwdToForeignAsync_ValidInput_ReturnsCorrectResult()
    {
        // Arrange
        var mockDataService = new Mock<ICurrencyDataService>();
        mockDataService.Setup(x => x.LoadAsync())
            .ReturnsAsync(new ExchangeRateData
            {
                LastFetchTime = DateTime.Now,
                Rates = new List<ExchangeRate>
                {
                    new ExchangeRate
                    {
                        CurrencyCode = "USD",
                        CurrencyName = "美元",
                        CashBuyRate = 31.200m,
                        CashSellRate = 31.600m,
                        LastUpdated = DateTime.Now
                    }
                }
            });

        var service = new CurrencyService(
            Mock.Of<HttpClient>(),
            mockDataService.Object,
            new MemoryCache(new MemoryCacheOptions()));

        // Act
        var result = await service.CalculateTwdToForeignAsync(10000m, "USD");

        // Assert
        Assert.Equal(316.455696m, result.OutputAmount, 6);
        Assert.Equal("TWD", result.SourceCurrency);
        Assert.Equal("USD", result.TargetCurrency);
        Assert.Equal(31.600m, result.ExchangeRate);
        Assert.Equal(CalculationDirection.TwdToForeign, result.Direction);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task CalculateTwdToForeignAsync_InvalidAmount_ThrowsArgumentException(decimal amount)
    {
        // Arrange
        var service = CreateMockService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CalculateTwdToForeignAsync(amount, "USD"));
    }

    [Fact]
    public async Task CalculateTwdToForeignAsync_UnsupportedCurrency_ThrowsArgumentException()
    {
        // Arrange
        var service = CreateMockService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CalculateTwdToForeignAsync(10000m, "XXX"));
    }

    private ICurrencyService CreateMockService()
    {
        var mockDataService = new Mock<ICurrencyDataService>();
        mockDataService.Setup(x => x.LoadAsync())
            .ReturnsAsync(new ExchangeRateData { Rates = new List<ExchangeRate>() });

        return new CurrencyService(
            Mock.Of<HttpClient>(),
            mockDataService.Object,
            new MemoryCache(new MemoryCacheOptions()));
    }
}
```

**執行測試**: ❌ 失敗（服務未實作）

#### 3.2 實作 CurrencyService

參考 `contracts/currency-service-api.md` 完整實作

**執行測試**: ✅ 通過

---

### Phase 4: 建立 Razor Page

#### 4.1 建立頁面測試

**檔案**: `BNICalculate.Tests/Integration/Pages/CurrencyConverterPageTests.cs`

```csharp
public class CurrencyConverterPageTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CurrencyConverterPageTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_ReturnsSuccessAndCorrectContentType()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/CurrencyConverter");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task Get_ContainsExpectedText()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/CurrencyConverter");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("匯率計算器", content);
        Assert.Contains("台幣轉外幣", content);
        Assert.Contains("外幣轉台幣", content);
    }
}
```

#### 4.2 建立 Razor Page

**檔案**: `BNICalculate/Pages/CurrencyConverter.cshtml`

**檔案**: `BNICalculate/Pages/CurrencyConverter.cshtml.cs`

參考 `contracts/currency-service-api.md` 的 PageModel 範例

---

### Phase 5: 服務註冊與設定

#### 5.1 更新 Program.cs

```csharp
// 在現有的 Program.cs 中新增
using Serilog;

// 設定 Serilog（在 builder 建立之前）
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/currency-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

// 使用 Serilog
builder.Host.UseSerilog();

// 註冊 Big5 編碼支援
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// 註冊 HttpClient
builder.Services.AddHttpClient<ICurrencyService, CurrencyService>(client =>
{
    client.BaseAddress = new Uri("https://rate.bot.com.tw");
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Add("User-Agent", "BNICalculate/1.0");
});

// 註冊服務
builder.Services.AddScoped<ICurrencyDataService, CurrencyDataService>();

// 註冊記憶體快取（如果尚未註冊）
builder.Services.AddMemoryCache();
```

#### 5.2 更新導覽選單

**檔案**: `BNICalculate/Pages/Shared/_Layout.cshtml`

```html
<li class="nav-item">
    <a class="nav-link text-dark" asp-area="" asp-page="/CurrencyConverter">匯率計算器</a>
</li>
```

---

## 執行與測試

### 執行所有測試

```bash
# 在方案根目錄
dotnet test

# 只執行單元測試
dotnet test --filter "Category!=Integration"

# 只執行整合測試
dotnet test --filter "Category=Integration"
```

### 執行應用程式

```bash
# 在 BNICalculate 專案目錄
dotnet run

# 或使用 watch 模式（自動重新載入）
dotnet watch run
```

### 瀏覽器測試

開啟瀏覽器訪問：<http://localhost:5000/CurrencyConverter>

---

## 常見問題排解

### 問題 1: Big5 編碼錯誤

**錯誤訊息**: `ArgumentException: 'big5' is not a supported encoding name`

**解決方案**:

```csharp
// 在 Program.cs 最開頭加入
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
```

### 問題 2: JSON 檔案權限錯誤

**錯誤訊息**: `UnauthorizedAccessException: Access to the path is denied`

**解決方案**:

```bash
# 確保 App_Data 目錄有寫入權限
chmod 755 BNICalculate/App_Data
```

### 問題 3: API 逾時

**錯誤訊息**: `TaskCanceledException: The request was canceled`

**解決方案**:

- 檢查網路連線
- 增加逾時時間（HttpClient.Timeout）
- 使用本地快取資料作為降級方案

### 問題 4: CSV 解析失敗

**錯誤訊息**: `CsvHelperException: Bad data found`

**解決方案**:

- 確認 Big5 編碼正確註冊
- 檢查 CSV 欄位映射是否正確
- 查看台銀 API 回應格式是否變更

---

## 效能優化建議

1. **啟用回應壓縮** (Program.cs):

   ```csharp
   builder.Services.AddResponseCompression(options =>
   {
       options.EnableForHttps = true;
   });
   ```

1. **啟用靜態檔案快取** (Program.cs):

   ```csharp
   app.UseStaticFiles(new StaticFileOptions
   {
       OnPrepareResponse = ctx =>
       {
           ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=3600");
       }
   });
   ```

1. **CSS/JS 最小化**: 使用 BuildBundlerMinifier 套件

---

## 下一步

完成開發後：

1. 執行完整測試套件確保覆蓋率 >80%
2. 執行 `dotnet build` 確認無編譯警告
3. 手動測試所有使用者情境
4. 建立 Pull Request 並請求程式碼審查
5. 更新專案 README.md 文件

---

## 參考資源

- [ASP.NET Core Razor Pages 文件](https://learn.microsoft.com/aspnet/core/razor-pages/)
- [xUnit 測試指南](https://xunit.net/docs/getting-started/netcore/cmdline)
- [CsvHelper 文件](https://joshclose.github.io/CsvHelper/)
- [System.Text.Json 文件](https://learn.microsoft.com/dotnet/standard/serialization/system-text-json-overview)

---

## 支援

遇到問題請：

1. 檢查日誌檔案（`logs/` 目錄）
2. 執行測試診斷問題
3. 查閱 `research.md` 和 `contracts/` 文件
4. 聯繫團隊尋求協助
