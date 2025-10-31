# Data Model: 台幣與外幣匯率計算器

**日期**: 2025年11月1日  
**目的**: 定義資料實體、驗證規則與狀態轉換

## 核心實體

### 1. ExchangeRate（匯率資料）

**用途**: 代表特定貨幣的匯率資訊

**欄位**:

| 欄位名稱 | 型別 | 必要 | 說明 | 驗證規則 |
|---------|------|------|------|---------|
| CurrencyCode | string | ✅ | 貨幣代碼（如 USD, JPY） | 3 個字元，大寫英文 |
| CurrencyName | string | ✅ | 貨幣名稱（如美元、日圓） | 非空白，繁體中文 |
| CashBuyRate | decimal | ✅ | 現金買入匯率 | >0，最多 6 位小數 |
| CashSellRate | decimal | ✅ | 現金賣出匯率 | >0，最多 6 位小數 |
| LastUpdated | DateTime | ✅ | 最後更新時間 | UTC+8 時區 |

**關聯**:

- 屬於 `ExchangeRateData` 集合

**C# 實作**:

```csharp
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
    /// 現金買入匯率（銀行買入外幣的價格）
    /// </summary>
    [Required]
    [Range(0.000001, 999999, ErrorMessage = "匯率必須為正數")]
    public decimal CashBuyRate { get; set; }

    /// <summary>
    /// 現金賣出匯率（銀行賣出外幣的價格）
    /// </summary>
    [Required]
    [Range(0.000001, 999999, ErrorMessage = "匯率必須為正數")]
    public decimal CashSellRate { get; set; }

    /// <summary>
    /// 最後更新時間（UTC+8）
    /// </summary>
    [Required]
    public DateTime LastUpdated { get; set; }
}
```

---

### 2. ExchangeRateData（匯率資料集合）

**用途**: 包裝所有貨幣的匯率資料及更新時間

**欄位**:

| 欄位名稱 | 型別 | 必要 | 說明 | 驗證規則 |
|---------|------|------|------|---------|
| Rates | List&lt;ExchangeRate&gt; | ✅ | 所有貨幣的匯率清單 | 至少包含 1 筆資料 |
| LastFetchTime | DateTime | ✅ | 資料取得時間 | UTC+8 時區 |
| DataSource | string | ✅ | 資料來源（固定為"台灣銀行"） | 非空白 |

**業務規則**:

- 同一貨幣代碼不可重複
- LastFetchTime 不可為未來時間
- 資料超過 24 小時視為過期（需顯示警告）

**C# 實作**:

```csharp
/// <summary>
/// 包裝所有貨幣的匯率資料及更新時間
/// </summary>
public class ExchangeRateData
{
    /// <summary>
    /// 所有貨幣的匯率清單
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "至少需要一筆匯率資料")]
    public List<ExchangeRate> Rates { get; set; } = new();

    /// <summary>
    /// 資料取得時間（UTC+8）
    /// </summary>
    [Required]
    public DateTime LastFetchTime { get; set; }

    /// <summary>
    /// 資料來源
    /// </summary>
    [Required]
    public string DataSource { get; set; } = "台灣銀行";

    /// <summary>
    /// 檢查資料是否過期（超過 24 小時）
    /// </summary>
    public bool IsStale()
    {
        var now = DateTime.Now;
        var age = now - LastFetchTime;
        return age.TotalHours > 24;
    }

    /// <summary>
    /// 根據貨幣代碼取得匯率
    /// </summary>
    public ExchangeRate? GetRate(string currencyCode)
    {
        return Rates.FirstOrDefault(r => 
            r.CurrencyCode.Equals(currencyCode, StringComparison.OrdinalIgnoreCase));
    }
}
```

---

### 3. Currency（貨幣）

**用途**: 定義支援的貨幣類型（列舉型態）

**支援貨幣**:

| 代碼 | 名稱 | 說明 |
|------|------|------|
| USD | 美元 | 美國貨幣 |
| JPY | 日圓 | 日本貨幣 |
| CNY | 人民幣 | 中國貨幣 |
| EUR | 歐元 | 歐洲貨幣 |
| GBP | 英鎊 | 英國貨幣 |
| HKD | 港幣 | 香港貨幣 |
| AUD | 澳幣 | 澳洲貨幣 |

**C# 實作**:

```csharp
/// <summary>
/// 支援的貨幣類型
/// </summary>
public enum Currency
{
    /// <summary>美元</summary>
    [Display(Name = "美元 (USD)")]
    USD,

    /// <summary>日圓</summary>
    [Display(Name = "日圓 (JPY)")]
    JPY,

    /// <summary>人民幣</summary>
    [Display(Name = "人民幣 (CNY)")]
    CNY,

    /// <summary>歐元</summary>
    [Display(Name = "歐元 (EUR)")]
    EUR,

    /// <summary>英鎊</summary>
    [Display(Name = "英鎊 (GBP)")]
    GBP,

    /// <summary>港幣</summary>
    [Display(Name = "港幣 (HKD)")]
    HKD,

    /// <summary>澳幣</summary>
    [Display(Name = "澳幣 (AUD)")]
    AUD
}

/// <summary>
/// Currency 列舉的擴充方法
/// </summary>
public static class CurrencyExtensions
{
    /// <summary>
    /// 取得貨幣顯示名稱
    /// </summary>
    public static string GetDisplayName(this Currency currency)
    {
        var type = typeof(Currency);
        var member = type.GetMember(currency.ToString());
        var attributes = member[0].GetCustomAttributes(typeof(DisplayAttribute), false);
        return ((DisplayAttribute)attributes[0]).Name ?? currency.ToString();
    }

    /// <summary>
    /// 取得所有支援的貨幣清單
    /// </summary>
    public static List<SelectListItem> GetAllCurrencies()
    {
        return Enum.GetValues<Currency>()
            .Select(c => new SelectListItem
            {
                Value = c.ToString(),
                Text = c.GetDisplayName()
            })
            .ToList();
    }
}
```

---

### 4. CalculationRequest（計算請求）

**用途**: 封裝使用者的計算請求

**欄位**:

| 欄位名稱 | 型別 | 必要 | 說明 | 驗證規則 |
|---------|------|------|------|---------|
| Amount | decimal | ✅ | 輸入金額 | >0，最多 999,999,999 |
| SourceCurrency | string | ✅ | 來源貨幣代碼 | 3 個字元，支援清單內 |
| TargetCurrency | string | ✅ | 目標貨幣代碼 | 3 個字元，支援清單內 |
| Direction | CalculationDirection | ✅ | 計算方向 | TwdToForeign 或 ForeignToTwd |

**業務規則**:

- SourceCurrency 不可等於 TargetCurrency
- 金額必須為正數
- 貨幣代碼必須在支援清單內

**C# 實作**:

```csharp
/// <summary>
/// 計算方向
/// </summary>
public enum CalculationDirection
{
    /// <summary>台幣轉外幣</summary>
    TwdToForeign,

    /// <summary>外幣轉台幣</summary>
    ForeignToTwd
}

/// <summary>
/// 封裝使用者的計算請求
/// </summary>
public class CalculationRequest
{
    /// <summary>
    /// 輸入金額
    /// </summary>
    [Required(ErrorMessage = "請輸入金額")]
    [Range(0.01, 999999999, ErrorMessage = "金額必須為正數且不超過 999,999,999")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 來源貨幣代碼
    /// </summary>
    [Required(ErrorMessage = "請選擇來源貨幣")]
    [StringLength(3, MinimumLength = 3)]
    public string SourceCurrency { get; set; } = string.Empty;

    /// <summary>
    /// 目標貨幣代碼
    /// </summary>
    [Required(ErrorMessage = "請選擇目標貨幣")]
    [StringLength(3, MinimumLength = 3)]
    public string TargetCurrency { get; set; } = string.Empty;

    /// <summary>
    /// 計算方向
    /// </summary>
    [Required]
    public CalculationDirection Direction { get; set; }

    /// <summary>
    /// 驗證來源與目標貨幣不相同
    /// </summary>
    public bool IsValid()
    {
        return !SourceCurrency.Equals(TargetCurrency, StringComparison.OrdinalIgnoreCase);
    }
}
```

---

### 5. CalculationResult（計算結果）

**用途**: 封裝計算後的結果

**欄位**:

| 欄位名稱 | 型別 | 必要 | 說明 | 驗證規則 |
|---------|------|------|------|---------|
| InputAmount | decimal | ✅ | 輸入金額 | >0 |
| OutputAmount | decimal | ✅ | 輸出金額 | >0，保留 6 位小數 |
| SourceCurrency | string | ✅ | 來源貨幣代碼 | 3 個字元 |
| TargetCurrency | string | ✅ | 目標貨幣代碼 | 3 個字元 |
| ExchangeRate | decimal | ✅ | 使用的匯率 | >0 |
| CalculatedAt | DateTime | ✅ | 計算時間 | UTC+8 時區 |
| Direction | CalculationDirection | ✅ | 計算方向 | TwdToForeign 或 ForeignToTwd |

**C# 實作**:

```csharp
/// <summary>
/// 封裝計算後的結果
/// </summary>
public class CalculationResult
{
    /// <summary>
    /// 輸入金額
    /// </summary>
    public decimal InputAmount { get; set; }

    /// <summary>
    /// 輸出金額（保留 6 位小數）
    /// </summary>
    public decimal OutputAmount { get; set; }

    /// <summary>
    /// 來源貨幣代碼
    /// </summary>
    public string SourceCurrency { get; set; } = string.Empty;

    /// <summary>
    /// 目標貨幣代碼
    /// </summary>
    public string TargetCurrency { get; set; } = string.Empty;

    /// <summary>
    /// 使用的匯率
    /// </summary>
    public decimal ExchangeRate { get; set; }

    /// <summary>
    /// 計算時間（UTC+8）
    /// </summary>
    public DateTime CalculatedAt { get; set; }

    /// <summary>
    /// 計算方向
    /// </summary>
    public CalculationDirection Direction { get; set; }

    /// <summary>
    /// 格式化輸出結果（含貨幣符號）
    /// </summary>
    public string GetFormattedResult()
    {
        return Direction == CalculationDirection.TwdToForeign
            ? $"NT$ {InputAmount:N2} = {OutputAmount:N6} {TargetCurrency}"
            : $"{InputAmount:N6} {SourceCurrency} = NT$ {OutputAmount:N2}";
    }
}
```

---

## 資料關聯圖

```text
ExchangeRateData
├── LastFetchTime: DateTime
├── DataSource: string
└── Rates: List<ExchangeRate>
    ├── [0] ExchangeRate (USD)
    │   ├── CurrencyCode: "USD"
    │   ├── CurrencyName: "美元"
    │   ├── CashBuyRate: 31.200
    │   ├── CashSellRate: 31.600
    │   └── LastUpdated: DateTime
    ├── [1] ExchangeRate (JPY)
    └── ...

CalculationRequest
├── Amount: 10000
├── SourceCurrency: "TWD"
├── TargetCurrency: "USD"
└── Direction: TwdToForeign
            ↓
        [計算服務]
            ↓
CalculationResult
├── InputAmount: 10000
├── OutputAmount: 316.456000
├── ExchangeRate: 31.600
├── CalculatedAt: DateTime
└── Direction: TwdToForeign
```

---

## 狀態轉換

### 匯率資料生命週期

```text
[無資料] 
    ↓ 首次載入/手動更新
[更新中]
    ↓ 成功取得 API 資料
[已快取] (30 分鐘)
    ↓ 快取過期或手動更新
[更新中]
    ↓ 成功
[已快取] (更新時間戳記)
    ↓ 超過 24 小時
[過期警告] (但仍可使用)
    ↓ 手動更新
[更新中]
    ↓ 失敗 (API 不可用)
[使用舊資料 + 錯誤提示]
```

### 計算請求處理流程

```text
[使用者輸入]
    ↓ 客戶端驗證
[驗證失敗] → [顯示錯誤訊息]
[驗證通過]
    ↓ 提交表單
[伺服器端驗證]
    ↓ 驗證失敗 → [ModelState.Error]
    ↓ 驗證通過
[檢查快取]
    ↓ 快取命中
[取得匯率資料]
    ↓ 快取未命中
[從檔案載入] → [寫入快取]
    ↓
[執行計算]
    ↓ 使用 decimal 精度計算
[回傳結果]
    ↓ 四捨五入至 6 位小數
[顯示結果]
```

---

## JSON 儲存格式

**檔案路徑**: `App_Data/currency/rates.json`

**格式範例**:

```json
{
  "LastFetchTime": "2025-11-01T14:30:00+08:00",
  "DataSource": "台灣銀行",
  "Rates": [
    {
      "CurrencyCode": "USD",
      "CurrencyName": "美元",
      "CashBuyRate": 31.200,
      "CashSellRate": 31.600,
      "LastUpdated": "2025-11-01T14:30:00+08:00"
    },
    {
      "CurrencyCode": "JPY",
      "CurrencyName": "日圓",
      "CashBuyRate": 0.2100,
      "CashSellRate": 0.2180,
      "LastUpdated": "2025-11-01T14:30:00+08:00"
    },
    {
      "CurrencyCode": "CNY",
      "CurrencyName": "人民幣",
      "CashBuyRate": 4.350,
      "CashSellRate": 4.480,
      "LastUpdated": "2025-11-01T14:30:00+08:00"
    }
  ]
}
```

**序列化設定**:

```csharp
var options = new JsonSerializerOptions
{
    WriteIndented = true,  // 易讀格式
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,  // 支援中文
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,  // 小駝峰命名
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull  // 忽略 null 值
};
```

---

## 驗證規則總結

### ExchangeRate

- ✅ CurrencyCode: 3 個大寫英文字母
- ✅ CashBuyRate/CashSellRate: >0，最多 6 位小數
- ✅ LastUpdated: 不可為未來時間

### CalculationRequest

- ✅ Amount: 0.01 ~ 999,999,999
- ✅ SourceCurrency ≠ TargetCurrency
- ✅ 貨幣代碼必須在支援清單內

### 業務邏輯驗證

- ✅ 資料過期檢查（24 小時）
- ✅ 快取時效性（30 分鐘）
- ✅ 四捨五入精度（6 位小數）
- ✅ 並行更新衝突（最後寫入勝出）

---

## 錯誤處理

### 資料層錯誤

| 錯誤類型 | 處理方式 | 使用者訊息 |
|---------|---------|----------|
| 檔案不存在 | 自動觸發更新 | 「正在載入最新匯率資料...」 |
| JSON 格式錯誤 | 記錄錯誤，顯示訊息 | 「資料格式異常，請更新匯率」 |
| API 連線失敗 | 使用快取資料 | 「無法連線至台銀 API，使用本地資料」 |
| CSV 解析失敗 | 記錄錯誤，使用快取 | 「資料解析失敗，請稍後再試」 |

### 計算層錯誤

| 錯誤類型 | 處理方式 | 使用者訊息 |
|---------|---------|----------|
| 貨幣代碼不存在 | 回傳 null | 「不支援的貨幣類型」 |
| 金額溢位 | 捕捉例外 | 「金額超過系統處理範圍」 |
| 匯率為 0 | 阻止計算 | 「匯率資料異常，請更新」 |

---

## 測試資料範例

### 有效的匯率資料

```csharp
var validExchangeRate = new ExchangeRate
{
    CurrencyCode = "USD",
    CurrencyName = "美元",
    CashBuyRate = 31.200m,
    CashSellRate = 31.600m,
    LastUpdated = DateTime.Now
};
```

### 有效的計算請求

```csharp
var validRequest = new CalculationRequest
{
    Amount = 10000m,
    SourceCurrency = "TWD",
    TargetCurrency = "USD",
    Direction = CalculationDirection.TwdToForeign
};
```

### 無效的計算請求（測試案例）

```csharp
// 金額為負數
var invalidAmount = new CalculationRequest { Amount = -100m };

// 來源與目標貨幣相同
var sameCurrency = new CalculationRequest 
{ 
    SourceCurrency = "USD", 
    TargetCurrency = "USD" 
};

// 不支援的貨幣
var unsupportedCurrency = new CalculationRequest 
{ 
    TargetCurrency = "XXX" 
};
```

---

## 總結

資料模型設計遵循以下原則：

1. **型別安全**: 使用強型別、列舉、Data Annotations
2. **驗證完整**: 伺服器端和客戶端雙重驗證
3. **業務邏輯封裝**: 方法如 `IsStale()`, `GetRate()`, `IsValid()`
4. **可測試性**: 清晰的輸入/輸出，易於單元測試
5. **文件完整**: XML 註解，繁體中文說明
6. **擴展性**: 易於新增貨幣或調整驗證規則

所有實體已準備就緒，可進入服務層設計與 API 合約定義。
