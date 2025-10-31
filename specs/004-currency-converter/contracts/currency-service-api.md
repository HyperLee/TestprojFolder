# API Contract: Currency Service (內部服務)

**日期**: 2025年11月1日  
**服務類型**: 內部應用程式服務（非 HTTP API）  
**目的**: 定義匯率服務的介面合約

## 服務介面

### ICurrencyService

**命名空間**: `BNICalculate.Services`  
**用途**: 提供匯率資料取得和計算功能

```csharp
/// <summary>
/// 匯率服務介面
/// </summary>
public interface ICurrencyService
{
    /// <summary>
    /// 從台灣銀行 API 取得最新匯率並更新本地資料
    /// </summary>
    /// <returns>更新後的匯率資料</returns>
    /// <exception cref="ExternalServiceException">API 呼叫失敗時擲出</exception>
    Task<ExchangeRateData> FetchAndUpdateRatesAsync();

    /// <summary>
    /// 取得匯率資料（優先從快取，快取未命中則從檔案載入）
    /// </summary>
    /// <returns>匯率資料，若無資料則回傳 null</returns>
    Task<ExchangeRateData?> GetRatesAsync();

    /// <summary>
    /// 計算台幣轉外幣
    /// </summary>
    /// <param name="twdAmount">台幣金額</param>
    /// <param name="currencyCode">目標外幣代碼</param>
    /// <returns>計算結果</returns>
    /// <exception cref="ArgumentException">貨幣代碼無效時擲出</exception>
    Task<CalculationResult> CalculateTwdToForeignAsync(decimal twdAmount, string currencyCode);

    /// <summary>
    /// 計算外幣轉台幣
    /// </summary>
    /// <param name="foreignAmount">外幣金額</param>
    /// <param name="currencyCode">來源外幣代碼</param>
    /// <returns>計算結果</returns>
    /// <exception cref="ArgumentException">貨幣代碼無效時擲出</exception>
    Task<CalculationResult> CalculateForeignToTwdAsync(decimal foreignAmount, string currencyCode);

    /// <summary>
    /// 檢查匯率資料是否過期（超過 24 小時）
    /// </summary>
    /// <returns>True 表示資料過期</returns>
    Task<bool> IsDataStaleAsync();
}
```

---

### ICurrencyDataService

**命名空間**: `BNICalculate.Services`  
**用途**: 提供匯率資料的持久化功能

```csharp
/// <summary>
/// 匯率資料存取服務介面
/// </summary>
public interface ICurrencyDataService
{
    /// <summary>
    /// 從 JSON 檔案載入匯率資料
    /// </summary>
    /// <returns>匯率資料，若檔案不存在則回傳 null</returns>
    Task<ExchangeRateData?> LoadAsync();

    /// <summary>
    /// 儲存匯率資料至 JSON 檔案
    /// </summary>
    /// <param name="data">要儲存的匯率資料</param>
    Task SaveAsync(ExchangeRateData data);

    /// <summary>
    /// 檢查資料檔案是否存在
    /// </summary>
    /// <returns>True 表示檔案存在</returns>
    Task<bool> ExistsAsync();

    /// <summary>
    /// 取得資料檔案最後修改時間
    /// </summary>
    /// <returns>最後修改時間，檔案不存在則回傳 null</returns>
    Task<DateTime?> GetLastModifiedTimeAsync();
}
```

---

## 方法規格

### FetchAndUpdateRatesAsync

**目的**: 從台銀 API 取得最新匯率並更新本地儲存

**前置條件**:

- HttpClient 已正確設定
- Big5 編碼提供者已註冊

**後置條件**:

- 成功時：本地 JSON 檔案更新，快取更新
- 失敗時：擲出 ExternalServiceException，本地資料不變

**執行流程**:

1. 呼叫台銀 API
2. 解析 CSV 資料
3. 驗證並轉換為 ExchangeRate 物件
4. 儲存至 JSON 檔案
5. 更新記憶體快取
6. 回傳 ExchangeRateData

**錯誤處理**:

| 錯誤情況 | 例外類型 | 訊息 |
|---------|---------|------|
| API 連線失敗 | ExternalServiceException | "無法連線至台灣銀行 API" |
| API 逾時 | ExternalServiceException | "匯率更新請求逾時" |
| CSV 解析失敗 | DataFormatException | "匯率資料格式異常" |
| 檔案寫入失敗 | IOException | "無法儲存匯率資料" |

**範例**:

```csharp
try
{
    var rates = await _currencyService.FetchAndUpdateRatesAsync();
    TempData["Message"] = $"匯率已更新，共 {rates.Rates.Count} 種貨幣";
}
catch (ExternalServiceException ex)
{
    _logger.LogError(ex, "更新匯率失敗");
    TempData["Error"] = "無法取得最新匯率資料，請稍後再試";
}
```

---

### GetRatesAsync

**目的**: 取得匯率資料（優先從快取）

**前置條件**: 無

**後置條件**:

- 快取命中：直接回傳快取資料
- 快取未命中：從檔案載入並更新快取
- 檔案不存在：回傳 null

**快取策略**:

- 快取鍵：`"CurrencyRates:Latest"`
- 滑動過期：30 分鐘
- 優先權：Normal

**執行流程**:

```csharp
public async Task<ExchangeRateData?> GetRatesAsync()
{
    // 1. 嘗試從快取讀取
    if (_cache.TryGetValue(CacheKey, out ExchangeRateData? cachedRates))
    {
        return cachedRates;
    }

    // 2. 從檔案載入
    var rates = await _dataService.LoadAsync();
    
    // 3. 更新快取
    if (rates != null)
    {
        _cache.Set(CacheKey, rates, new MemoryCacheEntryOptions
        {
            SlidingExpiration = CacheDuration,
            Priority = CacheItemPriority.Normal
        });
    }

    return rates;
}
```

**範例**:

```csharp
var rates = await _currencyService.GetRatesAsync();
if (rates == null)
{
    // 首次使用，觸發更新
    rates = await _currencyService.FetchAndUpdateRatesAsync();
}
```

---

### CalculateTwdToForeignAsync

**目的**: 計算台幣轉外幣

**前置條件**:

- twdAmount > 0
- currencyCode 在支援清單內
- 匯率資料可用

**後置條件**:

- 回傳 CalculationResult，OutputAmount 保留 6 位小數

**計算公式**:

```text
外幣金額 = 台幣金額 ÷ 現金賣出匯率
```

**四捨五入**: `Math.Round(result, 6, MidpointRounding.AwayFromZero)`

**錯誤處理**:

| 錯誤情況 | 例外類型 | 訊息 |
|---------|---------|------|
| 金額 ≤ 0 | ArgumentException | "金額必須為正數" |
| 貨幣代碼無效 | ArgumentException | "不支援的貨幣類型: {currencyCode}" |
| 匯率資料不存在 | InvalidOperationException | "匯率資料不可用" |

**範例**:

```csharp
// 計算 10,000 台幣可兌換多少美元
var result = await _currencyService.CalculateTwdToForeignAsync(10000m, "USD");

Console.WriteLine(result.GetFormattedResult());
// 輸出: "NT$ 10,000.00 = 316.456000 USD"
```

---

### CalculateForeignToTwdAsync

**目的**: 計算外幣轉台幣

**前置條件**:

- foreignAmount > 0
- currencyCode 在支援清單內
- 匯率資料可用

**後置條件**:

- 回傳 CalculationResult，OutputAmount 保留 6 位小數

**計算公式**:

```text
台幣金額 = 外幣金額 × 現金買入匯率
```

**四捨五入**: `Math.Round(result, 6, MidpointRounding.AwayFromZero)`

**錯誤處理**: 同 CalculateTwdToForeignAsync

**範例**:

```csharp
// 計算 100 美元可兌換多少台幣
var result = await _currencyService.CalculateForeignToTwdAsync(100m, "USD");

Console.WriteLine(result.GetFormattedResult());
// 輸出: "100.000000 USD = NT$ 3,120.00"
```

---

### IsDataStaleAsync

**目的**: 檢查匯率資料是否過期（超過 24 小時）

**前置條件**: 無

**後置條件**:

- 資料存在且 < 24 小時：回傳 false
- 資料存在但 ≥ 24 小時：回傳 true
- 資料不存在：回傳 true

**執行流程**:

```csharp
public async Task<bool> IsDataStaleAsync()
{
    var rates = await GetRatesAsync();
    
    if (rates == null)
    {
        return true; // 無資料視為過期
    }

    return rates.IsStale(); // 檢查 LastFetchTime
}
```

**範例**:

```csharp
if (await _currencyService.IsDataStaleAsync())
{
    ViewData["Warning"] = "⚠️ 注意：匯率資料已超過24小時未更新，建議點擊「更新匯率」取得最新資料";
}
```

---

## 服務實作

### CurrencyService 類別結構

```csharp
using Serilog;

public class CurrencyService : ICurrencyService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly ICurrencyDataService _dataService;
    private readonly IMemoryCache _cache;

    private const string CacheKey = "CurrencyRates:Latest";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public CurrencyService(
        HttpClient httpClient,
        ICurrencyDataService dataService,
        IMemoryCache cache)
    {
        _httpClient = httpClient;
        _logger = Log.ForContext<CurrencyService>();
        _dataService = dataService;
        _cache = cache;
    }

    // 實作 ICurrencyService 方法...
}
```

### CurrencyDataService 類別結構

```csharp
using Serilog;

public class CurrencyDataService : ICurrencyDataService
{
    private readonly ILogger _logger;
    private readonly string _dataFilePath;

    public CurrencyDataService(IWebHostEnvironment environment)
    {
        _logger = Log.ForContext<CurrencyDataService>();
        _dataFilePath = Path.Combine(
            environment.ContentRootPath,
            "App_Data",
            "currency",
            "rates.json");

        // 確保目錄存在
        Directory.CreateDirectory(Path.GetDirectoryName(_dataFilePath)!);
    }

    // 實作 ICurrencyDataService 方法...
}
```

---

## 服務註冊 (Program.cs)

```csharp
// 註冊 HttpClient
builder.Services.AddHttpClient<ICurrencyService, CurrencyService>(client =>
{
    client.BaseAddress = new Uri("https://rate.bot.com.tw");
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Add("User-Agent", "BNICalculate/1.0");
});

// 註冊資料服務
builder.Services.AddScoped<ICurrencyDataService, CurrencyDataService>();

// 註冊記憶體快取
builder.Services.AddMemoryCache();

// 註冊 Big5 編碼支援
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
```

---

## PageModel 整合

### CurrencyConverterModel 類別

```csharp
using Serilog;

public class CurrencyConverterModel : PageModel
{
    private readonly ICurrencyService _currencyService;
    private readonly ILogger _logger;

    [BindProperty]
    public decimal Amount { get; set; }

    [BindProperty]
    public string SelectedCurrency { get; set; } = string.Empty;

    [BindProperty]
    public string Direction { get; set; } = "TwdToForeign";

    public ExchangeRateData? CurrentRates { get; set; }
    public CalculationResult? Result { get; set; }
    public bool IsDataStale { get; set; }

    public CurrencyConverterModel(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
        _logger = Log.ForContext<CurrencyConverterModel>();
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // 載入匯率資料
        CurrentRates = await _currencyService.GetRatesAsync();
        
        // 首次載入，無資料時自動更新
        if (CurrentRates == null)
        {
            try
            {
                CurrentRates = await _currencyService.FetchAndUpdateRatesAsync();
                TempData["Success"] = "已成功載入最新匯率資料";
            }
            catch (ExternalServiceException ex)
            {
                _logger.LogError(ex, "首次載入匯率失敗");
                TempData["Error"] = "無法載入匯率資料，請檢查網路連線後重試";
                return Page();
            }
        }

        // 檢查資料時效
        IsDataStale = await _currencyService.IsDataStaleAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostCalculateAsync()
    {
        if (!ModelState.IsValid)
        {
            CurrentRates = await _currencyService.GetRatesAsync();
            return Page();
        }

        try
        {
            Result = Direction == "TwdToForeign"
                ? await _currencyService.CalculateTwdToForeignAsync(Amount, SelectedCurrency)
                : await _currencyService.CalculateForeignToTwdAsync(Amount, SelectedCurrency);

            CurrentRates = await _currencyService.GetRatesAsync();
            IsDataStale = await _currencyService.IsDataStaleAsync();

            return Page();
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            CurrentRates = await _currencyService.GetRatesAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostUpdateRatesAsync()
    {
        try
        {
            CurrentRates = await _currencyService.FetchAndUpdateRatesAsync();
            TempData["Success"] = $"匯率已更新，共 {CurrentRates.Rates.Count} 種貨幣";
        }
        catch (ExternalServiceException ex)
        {
            _logger.LogError(ex, "更新匯率失敗");
            TempData["Error"] = "⚠️ 無法取得最新匯率資料，目前使用本地快取資料，請稍後再試";
            CurrentRates = await _currencyService.GetRatesAsync();
        }

        IsDataStale = await _currencyService.IsDataStaleAsync();
        return Page();
    }
}
```

---

## 測試合約

### 單元測試範例

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
                Rates = new List<ExchangeRate>
                {
                    new ExchangeRate
                    {
                        CurrencyCode = "USD",
                        CashSellRate = 31.600m
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
        Assert.Equal(316.456329m, result.OutputAmount, 6);
        Assert.Equal("USD", result.TargetCurrency);
        Assert.Equal(31.600m, result.ExchangeRate);
    }

    [Fact]
    public async Task CalculateTwdToForeignAsync_InvalidCurrency_ThrowsArgumentException()
    {
        // Arrange
        var mockDataService = new Mock<ICurrencyDataService>();
        mockDataService.Setup(x => x.LoadAsync())
            .ReturnsAsync(new ExchangeRateData { Rates = new List<ExchangeRate>() });

        var service = new CurrencyService(
            Mock.Of<HttpClient>(),
            mockDataService.Object,
            new MemoryCache(new MemoryCacheOptions()));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CalculateTwdToForeignAsync(10000m, "XXX"));
    }
}
```

---

## 效能指標

| 操作 | 目標時間 | 測量方式 |
|------|---------|---------|
| GetRatesAsync (快取命中) | <10 毫秒 | Stopwatch |
| GetRatesAsync (快取未命中) | <100 毫秒 | Stopwatch |
| CalculateTwdToForeignAsync | <50 毫秒 | Stopwatch |
| FetchAndUpdateRatesAsync | <15 秒 | Stopwatch |

---

## 相依性總結

```csharp
// 外部相依
- HttpClient (IHttpClientFactory)
- Serilog (靜態日誌記錄器)
- IMemoryCache
- IWebHostEnvironment

// 內部相依
- ICurrencyDataService
- System.Text.Json
- CsvHelper
- System.Text.Encoding.CodePages
```

---

## 變更紀錄

| 日期 | 版本 | 變更內容 |
|------|------|---------|
| 2025-11-01 | 1.0 | 初版建立 |
