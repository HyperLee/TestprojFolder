# Research: 台幣與外幣匯率計算器

**日期**: 2025年11月1日  
**目的**: 解決技術選型與最佳實務研究

## 研究任務清單

### 1. 台灣銀行 CSV API 資料格式與存取模式

**決策**: 使用 HttpClient 搭配 IHttpClientFactory 呼叫台銀 CSV API

**理由**:

- 台銀 API 端點：<https://rate.bot.com.tw/xrt/flcsv/0/day>
- 回應格式：純文字 CSV，使用 Big5 編碼
- 無需身份驗證（公開服務）
- IHttpClientFactory 是 ASP.NET Core 推薦的 HTTP 客戶端管理方式，提供連線池管理和錯誤處理
- 設定 15 秒逾時符合規格需求

**替代方案評估**:

- ❌ WebClient：已過時，不建議在 .NET 8.0 使用
- ❌ RestSharp：額外相依性，對簡單 CSV 下載過度設計
- ✅ HttpClient + IHttpClientFactory：原生支援，效能佳，生命週期管理完善

**實作細節**:

```csharp
// Program.cs 註冊
builder.Services.AddHttpClient<ICurrencyService, CurrencyService>(client =>
{
    client.BaseAddress = new Uri("https://rate.bot.com.tw");
    client.Timeout = TimeSpan.FromSeconds(15);
});

// 編碼處理
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
var encoding = Encoding.GetEncoding("Big5");
```

---

### 2. CSV 解析與資料轉換策略

**決策**: 使用 CsvHelper 函式庫搭配 Big5 編碼處理

**理由**:

- CsvHelper 是 .NET 生態系統中最成熟的 CSV 解析函式庫
- 支援型別映射（Type Mapping），自動轉換為強型別物件
- 提供錯誤處理和資料驗證機制
- 效能優異，記憶體佔用低

**替代方案評估**:

- ❌ 手動 Split：容易出錯，無法處理引號內的逗號
- ❌ Regex 解析：複雜且難以維護
- ✅ CsvHelper：成熟、可靠、社群支援良好

**CSV 資料格式範例**:

```csv
幣別,現金買入,現金賣出,即期買入,即期賣出
USD,31.200,31.600,31.400,31.500
JPY,0.2100,0.2180,0.2140,0.2160
```

**型別映射**:

```csharp
public class TaiwanBankCsvRecord
{
    [Name("幣別")]
    public string CurrencyCode { get; set; } = string.Empty;
    
    [Name("現金買入")]
    public string CashBuyRate { get; set; } = string.Empty;
    
    [Name("現金賣出")]
    public string CashSellRate { get; set; } = string.Empty;
}
```

---

### 3. JSON 檔案儲存最佳實務

**決策**: 使用 System.Text.Json 搭配非同步檔案操作

**理由**:

- System.Text.Json 是 .NET 內建的高效能 JSON 序列化器
- 原生支援，無需額外套件
- 效能優於 Newtonsoft.Json（約 1.5-2 倍速度）
- 記憶體佔用更低

**儲存位置**: `App_Data/currency/rates.json`

- App_Data 目錄慣例用於應用程式資料
- 不在 wwwroot 下，避免直接 HTTP 存取
- 自動建立目錄結構

**檔案操作模式**:

```csharp
// 非同步寫入
var json = JsonSerializer.Serialize(rates, new JsonSerializerOptions 
{ 
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 支援中文
});
await File.WriteAllTextAsync(filePath, json);

// 非同步讀取
var json = await File.ReadAllTextAsync(filePath);
var rates = JsonSerializer.Deserialize<ExchangeRateData>(json);
```

**替代方案評估**:

- ❌ Newtonsoft.Json：額外相依性，效能較差
- ❌ 同步檔案操作：阻塞執行緒，違反 ASP.NET Core 最佳實務
- ✅ System.Text.Json + 非同步：原生、高效能、符合最佳實務

---

### 4. IMemoryCache 快取策略實作

**決策**: 使用 ASP.NET Core 內建 IMemoryCache 實作 30 分鐘滑動過期

**理由**:

- IMemoryCache 是 ASP.NET Core 推薦的記憶體快取解決方案
- 提供滑動過期（Sliding Expiration）和絕對過期（Absolute Expiration）
- 自動記憶體管理，避免記憶體洩漏
- 執行緒安全

**快取鍵設計**: `"CurrencyRates:Latest"`

**過期策略**:

- 滑動過期：30 分鐘無存取則過期
- 絕對過期：無（資料持續有效直到手動更新）
- 優先權：Normal（平衡記憶體使用與效能）

**實作模式**:

```csharp
private const string CacheKey = "CurrencyRates:Latest";
private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

public async Task<ExchangeRateData?> GetRatesAsync()
{
    // 嘗試從快取讀取
    if (_cache.TryGetValue(CacheKey, out ExchangeRateData? cachedRates))
    {
        return cachedRates;
    }

    // 從檔案載入
    var rates = await LoadFromFileAsync();
    
    // 寫入快取
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

**替代方案評估**:

- ❌ 靜態變數快取：無過期機制，記憶體洩漏風險
- ❌ Redis/分散式快取：過度設計，單機應用無需分散式
- ✅ IMemoryCache：簡單、高效、符合需求

---

### 5. Decimal 精度計算最佳實務

**決策**: 使用 decimal 型別搭配 Math.Round 進行所有金額計算

**理由**:

- decimal 提供 28-29 位有效數字，適合財務計算
- 避免 float/double 的二進位浮點數精度問題
- C# 原生支援，無需額外函式庫

**四捨五入模式**: `MidpointRounding.AwayFromZero`

- 符合一般財務慣例（.5 往上捨入）
- 與銀行計算邏輯一致

**計算範例**:

```csharp
public decimal CalculateForeignAmount(decimal twdAmount, decimal sellRate)
{
    var result = twdAmount / sellRate;
    return Math.Round(result, 6, MidpointRounding.AwayFromZero);
}

public decimal CalculateTwdAmount(decimal foreignAmount, decimal buyRate)
{
    var result = foreignAmount * buyRate;
    return Math.Round(result, 6, MidpointRounding.AwayFromZero);
}
```

**替代方案評估**:

- ❌ float/double：精度不足，會產生誤差
- ❌ 整數運算（以分為單位）：需要額外轉換邏輯
- ✅ decimal：原生支援，精度足夠，語意清晰

---

### 6. ASP.NET Core Razor Pages 表單驗證

**決策**: 使用 Data Annotations 搭配 jQuery Unobtrusive Validation

**理由**:

- 專案已包含 jQuery Validation（現有 BMI、Pomodoro 功能使用）
- 伺服器端和客戶端雙重驗證
- 支援即時回饋（離開欄位時驗證）

**驗證屬性**:

```csharp
public class CurrencyConverterModel : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "請輸入金額")]
    [Range(0.01, 999999999, ErrorMessage = "金額必須為正數且不超過 999,999,999")]
    public decimal Amount { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "請選擇貨幣")]
    public string CurrencyCode { get; set; } = string.Empty;
}
```

**客戶端驗證**:

```javascript
// 使用專案現有的 jQuery Validation
$("form").validate({
    rules: {
        Amount: {
            required: true,
            number: true,
            min: 0.01
        }
    },
    messages: {
        Amount: {
            required: "請輸入金額",
            number: "請輸入有效的數字",
            min: "金額必須為正數"
        }
    }
});
```

**替代方案評估**:

- ❌ 純 JavaScript 驗證：需重新實作，與現有專案不一致
- ❌ FluentValidation：額外相依性，簡單驗證不需要
- ✅ Data Annotations + jQuery Validation：與現有專案一致，簡單有效

---

### 7. 日誌記錄策略

**需求來源**: FR-009（記錄錯誤與警告事件）

**決策**: 使用 Serilog 搭配結構化日誌記錄

**理由**:

- Serilog 是 .NET 生態系中最受歡迎的結構化日誌函式庫
- 支援豐富的輸出目標（檔案、Console、資料庫、Seq 等）
- 強大的結構化日誌功能，便於查詢與分析
- 效能優異，支援非同步寫入
- 支援滾動檔案與自動清理舊日誌
- 日誌等級：Information（一般操作）、Warning（資料過時）、Error（API 失敗、檔案錯誤）

**實作**:

```csharp
// Program.cs - 設定 Serilog
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/currency-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7, // 保留最近7天日誌，自動刪除舊檔案
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// CurrencyService.cs - 使用日誌記錄
private readonly ILogger<CurrencyService> _logger;

public async Task<ExchangeRateData> FetchAndUpdateRatesAsync()
{
    _logger.LogInformation("開始從台銀 API 取得匯率資料");
    
    try
    {
        // API 呼叫
        _logger.LogInformation("成功取得匯率資料，共 {Count} 種貨幣", rates.Count);
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "台銀 API 呼叫失敗");
        throw;
    }
}
```

**套件需求**:

```bash
dotnet add package Serilog.AspNetCore --version 8.0.0
dotnet add package Serilog.Sinks.Console --version 5.0.1
dotnet add package Serilog.Sinks.File --version 5.0.0
```

**日誌保留策略**:

- **滾動間隔**: 每日滾動（`RollingInterval.Day`），檔案名稱格式：`currency-20251101.log`
- **保留數量**: 最近 7 天（`retainedFileCountLimit: 7`），自動刪除超過 7 天的舊日誌
- **理由**: 平衡磁碟空間與除錯需求，7 天足夠追蹤近期問題，同時避免日誌無限增長

**替代方案**:

- ❌ Console.WriteLine：無法設定等級，無結構化資訊
- ❌ Microsoft.Extensions.Logging 獨立使用：功能較陽春，需額外配置 Provider
- ✅ Serilog：功能完整、效能優異、社群支援強大

---

### 8. 響應式設計與 Bootstrap 5 整合

**決策**: 使用專案現有的 Bootstrap 5，遵循現有樣式模式

**理由**:

- 專案已包含 Bootstrap 5（BMI、Pomodoro 頁面使用）
- 提供完整的響應式 Grid 系統
- 包含表單樣式、按鈕、警告訊息等 UI 元件
- 確保跨頁面視覺一致性

**斷點設計**:

- **手機** (< 576px): 單欄佈局，輸入欄位全寬
- **平板** (≥ 576px): 兩欄佈局，左右各50%
- **桌面** (≥ 768px): 置中容器，最大寬度 800px

**Bootstrap 元件使用**:

```html
<!-- 表單群組 -->
<div class="mb-3">
    <label for="amount" class="form-label">金額</label>
    <input type="number" class="form-control" id="amount" />
    <span class="text-danger field-validation-error"></span>
</div>

<!-- 按鈕 -->
<button type="submit" class="btn btn-primary">
    <span class="spinner-border spinner-border-sm d-none"></span>
    計算
</button>

<!-- 警告訊息 -->
<div class="alert alert-warning" role="alert">
    ⚠️ 注意：匯率資料已超過24小時未更新
</div>
```

**自訂 CSS**:

- 建立 `wwwroot/css/currency-converter.css` 存放頁面特定樣式
- 遵循現有專案的 CSS 命名慣例
- 僅覆寫必要的 Bootstrap 樣式

**替代方案評估**:

- ❌ Tailwind CSS：需額外設定，與現有專案不一致
- ❌ 純手寫 CSS：開發時間長，響應式設計複雜
- ✅ Bootstrap 5：現有資源，一致性佳，開發快速

---

## 技術堆疊總結

| 技術層面 | 選擇 | 理由 |
|---------|------|------|
| HTTP 客戶端 | HttpClient + IHttpClientFactory | 原生支援，連線池管理，符合最佳實務 |
| CSV 解析 | CsvHelper | 成熟、可靠、型別映射支援 |
| JSON 序列化 | System.Text.Json | 高效能、原生支援、記憶體佔用低 |
| 快取機制 | IMemoryCache | 原生、執行緒安全、自動記憶體管理 |
| 金額計算 | decimal + Math.Round | 財務級精度、避免浮點誤差 |
| 表單驗證 | Data Annotations + jQuery Validation | 與現有專案一致、雙重驗證 |
| 日誌記錄 | Serilog | 功能完整、效能優異、結構化 |
| UI 框架 | Bootstrap 5 | 現有資源、響應式、一致性 |

---

## 相依套件清單

### 必要套件

```xml
<PackageReference Include="CsvHelper" Version="30.0.1" />
<PackageReference Include="System.Text.Encoding.CodePages" Version="8.0.0" />
```

### 測試套件（已存在於專案）

```xml
<PackageReference Include="xUnit" Version="2.6.2" />
<PackageReference Include="Moq" Version="4.20.70" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
```

---

## 效能考量

### 預期效能指標

| 操作 | 目標 | 策略 |
|------|------|------|
| 頁面載入 | <2 秒 | 靜態資源快取、CSS/JS 最小化 |
| 計算回應 | <3 秒 | 記憶體快取、非同步處理 |
| API 更新 | <15 秒 | HttpClient 逾時設定、錯誤處理 |
| JSON 讀取 | <100 毫秒 | 非同步檔案操作、System.Text.Json |
| 記憶體佔用 | <50 MB | IMemoryCache 自動管理、單一資料快取 |

### 最佳化策略

1. **30 分鐘快取**：減少 50% 以上的檔案讀取操作
2. **非同步操作**：所有 I/O 使用 async/await，避免執行緒阻塞
3. **僅保留最新資料**：覆蓋寫入，避免歷史資料累積
4. **壓縮回應**：啟用 Gzip/Brotli 壓縮（Program.cs 設定）
5. **延遲載入**：JavaScript/CSS 按需載入

---

## 風險與緩解措施

### 風險 1: 台銀 API 不可用

**緩解**:

- 本地 JSON 快取作為備援
- 清楚的錯誤訊息告知使用者
- 日誌記錄便於追蹤問題

### 風險 2: CSV 格式變更

**緩解**:

- CSV 欄位驗證機制
- 捕捉解析錯誤並記錄
- 使用快取資料作為降級方案

### 風險 3: 並行更新衝突

**緩解**:

- 允許並行更新，最後寫入勝出
- 檔案寫入使用原子操作（先寫暫存檔再 Move）
- 快取過期後重新載入最新資料

### 風險 4: Big5 編碼問題

**緩解**:

- 註冊 CodePagesEncodingProvider
- 錯誤處理和日誌記錄
- UTF-8 fallback 策略

---

## 結論

所有技術決策均基於以下原則：

1. **使用 .NET 生態系統原生工具**：最小化外部相依性
2. **遵循 ASP.NET Core 最佳實務**：async/await、DI、Serilog 結構化日誌
3. **與現有專案一致**：Bootstrap 5、jQuery、測試架構
4. **簡單性優於複雜性**：迷你專案，避免過度設計
5. **效能與可維護性平衡**：快取、非同步、清晰的程式碼結構

所有研究問題已解決，可進入 Phase 1 設計階段。
