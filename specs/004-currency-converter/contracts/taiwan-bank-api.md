# API Contract: 台灣銀行匯率 API

**日期**: 2025年11月1日  
**API 提供者**: 台灣銀行  
**API 類型**: 公開 REST API（無需認證）

## 基本資訊

**端點 URL**: <https://rate.bot.com.tw/xrt/flcsv/0/day>  
**HTTP 方法**: GET  
**回應格式**: CSV (Big5 編碼)  
**認證**: 無需認證  
**速率限制**: 無明確限制（建議間隔 30 秒以上）

## 請求規格

### HTTP Headers

```http
GET /xrt/flcsv/0/day HTTP/1.1
Host: rate.bot.com.tw
User-Agent: BNICalculate/1.0
Accept: text/csv
Accept-Encoding: gzip, deflate
```

### 查詢參數

無

### 請求範例 (C#)

```csharp
var httpClient = _httpClientFactory.CreateClient();
httpClient.BaseAddress = new Uri("https://rate.bot.com.tw");
httpClient.Timeout = TimeSpan.FromSeconds(15);

var response = await httpClient.GetAsync("/xrt/flcsv/0/day");
response.EnsureSuccessStatusCode();

// 使用 Big5 編碼讀取
var big5Encoding = Encoding.GetEncoding("Big5");
var csvContent = await response.Content.ReadAsStringAsync();
```

## 回應規格

### 成功回應 (200 OK)

**Content-Type**: `text/csv; charset=big5`

**CSV 欄位結構**:

| 欄位順序 | 欄位名稱 | 型別 | 說明 | 範例值 |
|---------|---------|------|------|--------|
| 1 | 幣別 | string | 貨幣代碼 | "USD" |
| 2 | 現金買入 | string | 現金買入匯率 | "31.200" |
| 3 | 現金賣出 | string | 現金賣出匯率 | "31.600" |
| 4 | 即期買入 | string | 即期買入匯率 | "31.400" |
| 5 | 即期賣出 | string | 即期賣出匯率 | "31.500" |

**CSV 範例**:

```csv
幣別,現金買入,現金賣出,即期買入,即期賣出
USD,31.200,31.600,31.400,31.500
JPY,0.2100,0.2180,0.2140,0.2160
CNY,4.350,4.480,4.400,4.450
EUR,33.800,34.500,34.100,34.300
GBP,39.500,40.300,39.800,40.100
HKD,3.980,4.120,4.020,4.080
AUD,20.500,21.000,20.700,20.900
```

**特殊值處理**:

- 空值或 "-"：表示該貨幣當日未提供匯率
- 非數字字元：視為無效資料，跳過該筆記錄

### 錯誤回應

#### 404 Not Found

```http
HTTP/1.1 404 Not Found
Content-Type: text/html
```

**處理方式**: 記錄錯誤，使用本地快取資料

#### 503 Service Unavailable

```http
HTTP/1.1 503 Service Unavailable
Content-Type: text/html
Retry-After: 60
```

**處理方式**: 記錄錯誤，使用本地快取資料，稍後重試

#### 逾時錯誤

**HttpClient Timeout**: 15 秒

**處理方式**: 捕捉 `TaskCanceledException`，使用本地快取資料

```csharp
try
{
    var response = await httpClient.GetAsync("/xrt/flcsv/0/day");
}
catch (TaskCanceledException ex)
{
    _logger.LogError(ex, "台灣銀行 API 請求逾時");
    // 使用本地快取資料
}
```

## 資料映射

### CSV 欄位 → C# 模型

```csharp
public class TaiwanBankCsvRecord
{
    [Name("幣別")]
    public string CurrencyCode { get; set; } = string.Empty;

    [Name("現金買入")]
    public string CashBuyRate { get; set; } = string.Empty;

    [Name("現金賣出")]
    public string CashSellRate { get; set; } = string.Empty;

    // 不使用即期匯率，可忽略這些欄位
}
```

### CSV → ExchangeRate 轉換邏輯

```csharp
public ExchangeRate? ParseCsvRecord(TaiwanBankCsvRecord record)
{
    // 驗證必要欄位
    if (string.IsNullOrWhiteSpace(record.CurrencyCode) ||
        string.IsNullOrWhiteSpace(record.CashBuyRate) ||
        string.IsNullOrWhiteSpace(record.CashSellRate))
    {
        return null;
    }

    // 解析匯率（處理可能的格式錯誤）
    if (!decimal.TryParse(record.CashBuyRate, out var buyRate) ||
        !decimal.TryParse(record.CashSellRate, out var sellRate))
    {
        _logger.LogWarning("無法解析匯率: {CurrencyCode}", record.CurrencyCode);
        return null;
    }

    // 驗證匯率為正數
    if (buyRate <= 0 || sellRate <= 0)
    {
        _logger.LogWarning("匯率為非正數: {CurrencyCode}", record.CurrencyCode);
        return null;
    }

    return new ExchangeRate
    {
        CurrencyCode = record.CurrencyCode.ToUpperInvariant(),
        CurrencyName = GetCurrencyName(record.CurrencyCode),
        CashBuyRate = buyRate,
        CashSellRate = sellRate,
        LastUpdated = DateTime.Now
    };
}

private string GetCurrencyName(string code)
{
    return code.ToUpperInvariant() switch
    {
        "USD" => "美元",
        "JPY" => "日圓",
        "CNY" => "人民幣",
        "EUR" => "歐元",
        "GBP" => "英鎊",
        "HKD" => "港幣",
        "AUD" => "澳幣",
        _ => code
    };
}
```

## 錯誤處理策略

### 1. 網路連線錯誤

```csharp
try
{
    var response = await httpClient.GetAsync("/xrt/flcsv/0/day");
    response.EnsureSuccessStatusCode();
}
catch (HttpRequestException ex)
{
    _logger.LogError(ex, "無法連線至台灣銀行 API");
    throw new ExternalServiceException("無法取得最新匯率資料", ex);
}
```

### 2. 逾時錯誤

```csharp
catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
{
    _logger.LogError(ex, "台灣銀行 API 請求逾時");
    throw new ExternalServiceException("匯率更新請求逾時", ex);
}
```

### 3. 編碼錯誤

```csharp
try
{
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    var big5 = Encoding.GetEncoding("Big5");
}
catch (ArgumentException ex)
{
    _logger.LogError(ex, "無法載入 Big5 編碼");
    throw new InvalidOperationException("系統編碼設定錯誤", ex);
}
```

### 4. CSV 解析錯誤

```csharp
try
{
    using var reader = new StringReader(csvContent);
    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
    var records = csv.GetRecords<TaiwanBankCsvRecord>().ToList();
}
catch (CsvHelperException ex)
{
    _logger.LogError(ex, "CSV 解析失敗");
    throw new DataFormatException("匯率資料格式異常", ex);
}
```

## 測試策略

### Mock 回應（單元測試）

```csharp
[Fact]
public async Task FetchRatesAsync_SuccessfulResponse_ReturnsParsedRates()
{
    // Arrange
    var mockHandler = new Mock<HttpMessageHandler>();
    var csvContent = "幣別,現金買入,現金賣出,即期買入,即期賣出\nUSD,31.200,31.600,31.400,31.500";
    
    mockHandler.Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(csvContent, Encoding.GetEncoding("Big5"))
        });

    var httpClient = new HttpClient(mockHandler.Object)
    {
        BaseAddress = new Uri("https://rate.bot.com.tw")
    };

    // Act
    var result = await _service.FetchRatesAsync();

    // Assert
    Assert.NotNull(result);
    Assert.Single(result.Rates);
    Assert.Equal("USD", result.Rates[0].CurrencyCode);
}
```

### 整合測試（實際 API 呼叫）

```csharp
[Fact]
[Trait("Category", "Integration")]
public async Task FetchRatesAsync_RealApi_ReturnsValidData()
{
    // Arrange
    var service = new CurrencyService(
        _httpClientFactory,
        _logger,
        _dataService,
        _cache);

    // Act
    var result = await service.FetchRatesAsync();

    // Assert
    Assert.NotNull(result);
    Assert.NotEmpty(result.Rates);
    Assert.True(result.Rates.Count >= 7); // 至少 7 種貨幣
    Assert.All(result.Rates, rate =>
    {
        Assert.True(rate.CashBuyRate > 0);
        Assert.True(rate.CashSellRate > 0);
    });
}
```

## 效能考量

### 快取策略

- **快取時長**: 30 分鐘（IMemoryCache 滑動過期）
- **快取鍵**: `"CurrencyRates:Latest"`
- **快取優先**: 優先使用快取，避免頻繁 API 呼叫

### 逾時設定

- **連線逾時**: 15 秒
- **讀取逾時**: 15 秒（包含在 HttpClient.Timeout 內）

### 重試策略

- **不實作自動重試**：避免對台銀 API 造成負擔
- **降級策略**：API 失敗時使用本地快取資料

## 相依性

### NuGet 套件

```xml
<PackageReference Include="CsvHelper" Version="30.0.1" />
<PackageReference Include="System.Text.Encoding.CodePages" Version="8.0.0" />
```

### 服務註冊 (Program.cs)

```csharp
// 註冊 HttpClient
builder.Services.AddHttpClient<ICurrencyService, CurrencyService>(client =>
{
    client.BaseAddress = new Uri("https://rate.bot.com.tw");
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Add("User-Agent", "BNICalculate/1.0");
});

// 註冊編碼提供者（Big5 支援）
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
```

## SLA 與限制

| 項目 | 值 | 說明 |
|------|---|------|
| 可用性 | 未公開 | 台銀 API 無 SLA 保證 |
| 回應時間 | < 5 秒 | 一般情況下 |
| 資料更新頻率 | 每工作日 | 非即時更新 |
| 速率限制 | 無明確限制 | 建議間隔 30 秒以上 |
| 資料保留期 | 當日 | 僅提供當日匯率 |

## 注意事項

1. **編碼問題**: 必須使用 Big5 編碼，否則中文亂碼
2. **資料時效**: 台銀匯率非即時更新，工作日才有新資料
3. **無 SLA**: 公開 API 無服務保證，必須有降級策略
4. **CSV 格式**: 欄位順序和名稱可能變更，需定期驗證
5. **假日資料**: 週末和假日可能無新資料

## 變更紀錄

| 日期 | 版本 | 變更內容 |
|------|------|---------|
| 2025-11-01 | 1.0 | 初版建立 |
