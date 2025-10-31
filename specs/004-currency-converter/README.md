# 💱 匯率計算器 (Currency Converter)

基於臺灣銀行即時匯率的台幣與外幣雙向轉換工具,支援 7 種主要貨幣。

## ✨ 功能特色

### 核心功能
- 🔄 **雙向轉換**: 台幣轉外幣 / 外幣轉台幣
- 💰 **多幣別支援**: 美元(USD)、日圓(JPY)、人民幣(CNY)、歐元(EUR)、英鎊(GBP)、港幣(HKD)、澳幣(AUD)
- 🏦 **即時匯率**: 自動從臺灣銀行 API 取得最新匯率資料
- 💾 **快取機制**: 匯率資料本地快取 24 小時,減少 API 呼叫
- 📊 **精確計算**: 支援最多 6 位小數精度

### 使用者體驗
- 📱 **響應式設計**: 支援手機、平板、桌面裝置 (Mobile-first)
- ♿ **無障礙支援**: 符合 WCAG 2.1 AA 標準,完整 ARIA 屬性
- ⌨️ **鍵盤導航**: 支援 Tab、Enter 鍵操作,包含 Skip Navigation
- 🌓 **深色模式**: 自動偵測系統主題偏好
- ✨ **即時驗證**: 表單輸入即時回饋與錯誤提示
- 🎨 **動畫效果**: 流暢的 Fade-in 和 Shake 動畫

## 🚀 快速開始

### 環境需求
- .NET 8.0 SDK
- ASP.NET Core 8.0
- 支援的瀏覽器: Chrome, Firefox, Safari, Edge (最新版本)

### 安裝與執行

1. **Clone 專案**
   ```bash
   git clone <repository-url>
   cd TestprojFolder
   ```

2. **還原套件**
   ```bash
   dotnet restore
   ```

3. **執行應用程式**
   ```bash
   cd BNICalculate
   dotnet run
   ```

4. **開啟瀏覽器**
   ```
   https://localhost:5001/CurrencyConverter
   ```

### 執行測試

```bash
# 執行所有測試
dotnet test

# 執行特定測試類別
dotnet test --filter "FullyQualifiedName~CurrencyServiceTests"

# 執行整合測試
dotnet test --filter "FullyQualifiedName~Integration"

# 查看測試覆蓋率
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## 📖 使用指南

### 台幣轉外幣

1. 在左側表單輸入台幣金額 (最小 0.01)
2. 選擇目標外幣幣別
3. 點擊「💹 計算」按鈕
4. 查看轉換結果與當前匯率

**範例**: 
- 輸入: 1000 TWD → 選擇 USD
- 結果: 約 31.65 美元 (依當前匯率)

### 外幣轉台幣

1. 在右側表單輸入外幣金額 (最小 0.01)
2. 選擇來源外幣幣別
3. 點擊「💱 計算」按鈕
4. 查看轉換結果與當前匯率

**範例**:
- 輸入: 100 USD → 選擇 USD
- 結果: 約 3100 台幣 (依當前匯率)

### 更新匯率

- 點擊頁面上方的「🔄 更新匯率」按鈕
- 系統會從臺灣銀行 API 取得最新匯率
- 更新完成後會顯示成功訊息
- 匯率資料會快取 24 小時

### 鍵盤快捷鍵

- `Tab`: 在欄位間移動
- `Enter`: 提交表單 / 跳到下一個欄位
- `Shift + Tab`: 向後移動焦點
- 頁面載入後按 `Tab`: 出現 "跳至主要內容" 連結

## 🏗️ 技術架構

### 前端技術
- **框架**: ASP.NET Core 8.0 Razor Pages
- **CSS 框架**: Bootstrap 5.3
- **JavaScript**: jQuery 3.7 + 原生 JavaScript
- **樣式**: 自定義 CSS (currency-converter.css)
- **響應式斷點**: 576px (手機), 768px (平板), 992px (桌面)

### 後端技術
- **語言**: C# 12
- **框架**: .NET 8.0 + ASP.NET Core 8.0
- **快取**: IMemoryCache (記憶體快取)
- **資料儲存**: JSON 檔案 (App_Data/currency/rates.json)
- **API 整合**: 臺灣銀行牌告匯率 API

### 測試框架
- **單元測試**: xUnit 2.5.3
- **Mock 框架**: Moq 4.20.69
- **整合測試**: WebApplicationFactory
- **測試覆蓋率**: 120+ 測試案例

### 專案結構

```
BNICalculate/
├── Pages/
│   ├── CurrencyConverter.cshtml         # 匯率計算器頁面
│   ├── CurrencyConverter.cshtml.cs      # 頁面邏輯 (PageModel)
│   └── Shared/
│       └── _Layout.cshtml               # 共用版面配置
├── Services/
│   ├── CurrencyService.cs               # 匯率計算服務
│   ├── CurrencyDataService.cs           # 資料存取服務
│   └── ICurrencyService.cs              # 服務介面
├── Models/
│   ├── ExchangeRate.cs                  # 匯率模型
│   ├── ExchangeRateData.cs              # 匯率資料容器
│   ├── CalculationRequest.cs            # 計算請求模型
│   └── CalculationResult.cs             # 計算結果模型
├── wwwroot/
│   ├── css/
│   │   └── currency-converter.css       # 自定義樣式
│   └── js/
│       └── currency-converter.js        # 前端邏輯
└── App_Data/
    └── currency/
        └── rates.json                   # 匯率資料快取

BNICalculate.Tests/
├── Unit/
│   ├── Services/
│   │   ├── CurrencyServiceTests.cs      # 服務單元測試
│   │   └── CurrencyDataServiceTests.cs  # 資料服務測試
│   └── Models/
│       ├── ExchangeRateTests.cs         # 模型測試
│       └── CalculationResultTests.cs    # 結果模型測試
├── Integration/
│   └── Pages/
│       └── CurrencyConverterPageTests.cs # 頁面整合測試
└── Services/
    ├── CurrencyServiceMultiCurrencyTests.cs  # 多幣別測試
    └── CurrencyServiceRateUpdateTests.cs     # 匯率更新測試
```

## 🎨 無障礙功能

本專案遵循 **WCAG 2.1 AA** 標準,提供完整的無障礙支援:

### ARIA 屬性
- ✅ `aria-label`: 所有表單欄位和按鈕
- ✅ `aria-required`: 必填欄位標示
- ✅ `aria-describedby`: 輸入提示和錯誤訊息
- ✅ `aria-live`: 動態內容更新通知
- ✅ `role="alert"`: 錯誤訊息容器
- ✅ `role="status"`: 成功訊息容器

### 鍵盤導航
- ✅ Skip to main content 連結
- ✅ 邏輯性的 Tab 順序
- ✅ Enter 鍵提交表單
- ✅ 可見的焦點指示器 (3px 藍色外框)

### 視覺輔助
- ✅ 高對比模式支援
- ✅ 深色模式支援
- ✅ 文字可縮放至 200%
- ✅ 充足的顏色對比度 (符合 WCAG AA)

### 螢幕閱讀器
- ✅ 完整的表單標籤
- ✅ 錯誤訊息語意化
- ✅ 動態內容即時通知
- ✅ 圖示的替代文字

## 📊 API 資料來源

### 臺灣銀行牌告匯率 API

- **URL**: `https://rate.bot.com.tw/xrt/fldjson/ltm/day`
- **更新頻率**: 即時更新 (工作日)
- **資料格式**: JSON
- **快取策略**: 本地快取 24 小時

### 匯率資料結構

```json
{
  "dataSource": "臺灣銀行",
  "lastFetchTime": "2025-11-01T10:30:00+08:00",
  "rates": [
    {
      "currencyCode": "USD",
      "currencyName": "美元",
      "cashBuyRate": 30.5,
      "cashSellRate": 31.0,
      "lastUpdated": "2025-11-01T10:00:00+08:00"
    }
  ]
}
```

### 支援的匯率類型

- **現金買入匯率** (Cash Buy Rate): 銀行買入外幣的價格
- **現金賣出匯率** (Cash Sell Rate): 銀行賣出外幣的價格

**計算邏輯**:
- 台幣轉外幣: 使用「現金買入匯率」(對客戶較有利)
- 外幣轉台幣: 使用「現金賣出匯率」(對客戶較有利)

## 🧪 測試覆蓋率

### 測試統計
- **總測試數**: 120+ 測試案例
- **單元測試**: 80+ 測試
- **整合測試**: 20+ 測試
- **多幣別測試**: 17 測試 (Theory)
- **測試通過率**: 100%

### 測試類別

#### 單元測試 (Unit Tests)
- ✅ CurrencyService: 計算邏輯、資料驗證、例外處理
- ✅ CurrencyDataService: 檔案讀寫、JSON 序列化
- ✅ Models: 資料模型驗證、格式化輸出
- ✅ Helpers: 日期時間處理

#### 整合測試 (Integration Tests)
- ✅ 頁面載入測試
- ✅ 表單提交測試
- ✅ 驗證錯誤測試
- ✅ 完整使用者流程測試
- ✅ 效能測試 (頁面載入 < 5s, 計算 < 3s)
- ✅ 安全性測試 (邊界值、不支援貨幣)

#### 多幣別測試 (Multi-Currency Tests)
- ✅ 7 種貨幣的台幣轉外幣測試
- ✅ 7 種貨幣的外幣轉台幣測試
- ✅ 取得所有貨幣列表
- ✅ 不支援貨幣例外處理

## ⚡ 效能指標

### 目標效能
- 📄 頁面載入時間: < 2 秒
- 🧮 計算執行時間: < 3 秒
- 🌐 API 呼叫時間: < 15 秒
- 💾 快取命中率: > 95%

### 優化策略
- ✅ 記憶體快取 (IMemoryCache)
- ✅ 24 小時資料快取
- ✅ 原子寫入 (Atomic Write) 防止資料損壞
- ✅ 異步操作 (async/await)
- ✅ CSS/JS 最小化 (生產環境)

## 🔒 安全性

### 輸入驗證
- ✅ 金額範圍驗證 (0.01 ~ 999,999,999)
- ✅ 貨幣代碼白名單驗證
- ✅ 模型驗證 (Data Annotations)
- ✅ XSS 防護 (Razor 自動編碼)

### API 安全
- ✅ HTTPS 強制執行
- ✅ CORS 設定
- ✅ 請求逾時處理
- ✅ 錯誤訊息脫敏

## 🐛 已知問題與限制

1. **匯率時效性**: 非即時匯率,快取 24 小時後才更新
2. **計算精度**: 最多支援 6 位小數,超過會四捨五入
3. **離線模式**: 無網路時無法更新匯率
4. **假日匯率**: 假日無法取得最新匯率,使用最後快取資料

## 📝 版本歷史

### v1.0.0 (2025-11-01)
- ✨ 初始版本發布
- ✅ 7 種貨幣支援 (USD, JPY, CNY, EUR, GBP, HKD, AUD)
- ✅ 雙向轉換功能
- ✅ 臺灣銀行 API 整合
- ✅ 響應式設計 + 無障礙支援
- ✅ 120+ 測試案例

## 🤝 貢獻指南

歡迎貢獻程式碼、回報問題或提出新功能建議!

### 開發流程
1. Fork 本專案
2. 建立功能分支 (`git checkout -b feature/amazing-feature`)
3. 提交變更 (`git commit -m 'Add some amazing feature'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 開啟 Pull Request

### 程式碼規範
- 遵循 C# 官方程式碼規範
- 所有公開方法必須有 XML 文件註解
- 單元測試覆蓋率需達 80% 以上
- 提交訊息使用語意化格式

## 📄 授權

本專案採用 MIT 授權條款 - 詳見 [LICENSE](LICENSE) 檔案

## 📧 聯絡資訊

- **專案連結**: [GitHub Repository](https://github.com/HyperLee/TestprojFolder)
- **問題回報**: [GitHub Issues](https://github.com/HyperLee/TestprojFolder/issues)

## 🙏 致謝

- [臺灣銀行](https://www.bot.com.tw/) - 提供即時匯率 API
- [Bootstrap](https://getbootstrap.com/) - UI 框架
- [jQuery](https://jquery.com/) - JavaScript 函式庫
- [xUnit](https://xunit.net/) - 測試框架

---

⭐ 如果這個專案對您有幫助,請給我們一個 Star!
