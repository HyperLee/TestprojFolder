# BNI Calculate

BMI 計算器、番茄工作法計時器、世界時鐘與匯率計算器網頁應用程式

## 功能

### BMI 計算器

- **輸入身高和體重**：使用者可輸入身高（公尺）和體重（公斤）
- **即時計算**：點擊「計算」按鈕，系統立即計算 BMI 值並顯示分類
- **WHO 標準分類**：根據 WHO 標準顯示體重分類（過輕、正常、過重、輕度肥胖、中度肥胖、重度肥胖）
- **輸入驗證**：自動驗證輸入資料，顯示清晰的錯誤訊息
- **清除功能**：點擊「清除」按鈕可快速重置所有欄位

### 番茄工作法計時器

- **25/5 分鐘計時循環**：25 分鐘工作時段 + 5 分鐘休息時段
- **自動階段切換**：工作完成後自動進入休息時段
- **計時器控制**：開始、暫停、繼續、重置功能
- **圓形進度環**：視覺化顯示時段完成進度
- **狀態恢復**：關閉頁面後重新開啟，計時器繼續執行
- **今日統計**：追蹤每日完成的番茄鐘數量
- **自訂時長設定**：調整工作/休息時長（工作：1-60 分鐘，休息：1-30 分鐘）
- **多視窗偵測**：防止同時開啟多個計時器視窗造成衝突
- **Toast 通知**：時段完成時顯示友善提醒

### 世界時鐘

- **10 個主要城市時間**：同時顯示台北、東京、倫敦、紐約、洛杉磯、巴黎、柏林、莫斯科、新加坡、悉尼的即時時間
- **自動夏令時間處理**：使用瀏覽器原生 Intl API 自動處理所有城市的夏令時間轉換
- **互動式城市切換**：點選任何城市卡片將其設為主要顯示，並以大字體置中顯示
- **即時更新**：每秒自動更新所有城市時間，確保時間準確
- **響應式設計**：自動適應桌面（3x3 網格）、平板（2 欄）、手機（1 欄）
- **無障礙設計**：支援鍵盤導覽（Tab + Enter）、螢幕閱讀器（ARIA 標籤）
- **心跳檢測**：自動偵測計時器停止並重啟，確保長時間執行穩定性
- **視覺回饋**：滑鼠懸停效果、點選動畫、焦點指示器
- **瀏覽器相容性檢查**：自動偵測並提示升級過舊的瀏覽器

### 匯率計算器 (NEW!)

- **雙向匯率轉換**：支援台幣轉外幣、外幣轉台幣兩種計算模式
- **7 種主要貨幣**：美元 (USD)、日圓 (JPY)、人民幣 (CNY)、歐元 (EUR)、英鎊 (GBP)、港幣 (HKD)、澳幣 (AUD)
- **即時匯率資料**：自動從台灣銀行 API 取得最新匯率（現金買入/賣出價）
- **智慧快取機制**：30 分鐘記憶體快取，減少 API 呼叫次數，提升效能
- **資料持久化**：匯率資料自動儲存至 JSON 檔案，離線時仍可使用
- **手動更新功能**：提供「🔄 更新匯率」按鈕，隨時取得最新匯率
- **資料時效提醒**：超過 24 小時的匯率資料會顯示警告訊息
- **精準計算**：計算結果保留 6 位小數，顯示 2 位小數（更易閱讀）
- **完整錯誤處理**：網路錯誤、資料格式錯誤、參數驗證等多層次錯誤處理
- **詳細日誌記錄**：使用 Serilog 記錄所有操作和錯誤，方便追蹤問題
- **響應式 UI**：使用 Bootstrap 5 設計，自動適應各種螢幕尺寸
- **無障礙設計**：完整的 ARIA 標籤和鍵盤導覽支援
- **即時顯示資訊**：匯率來源、最後更新時間、計算時間等完整資訊

## 技術堆疊

- **後端**: ASP.NET Core 8.0 Razor Pages
- **前端**: 原生 JavaScript + CSS（無框架），Bootstrap 5
- **資料儲存**: JSON 檔案（App_Data/pomodoro/、App_Data/currency/）
- **快取**: IMemoryCache
- **日誌**: Serilog（檔案輸出）
- **HTTP 客戶端**: IHttpClientFactory
- **CSV 解析**: CsvHelper
- **測試**: xUnit + WebApplicationFactory

## 快速開始

### 先決條件

- .NET SDK 8.0 或更高版本

### 執行應用程式

```bash
dotnet run --project BNICalculate
```

訪問：

- BMI 計算器: <http://localhost:5087/BMI>
- 番茄鐘計時器: <http://localhost:5087/Pomodoro>
- 世界時鐘: <http://localhost:5087/WorldClock>
- 匯率計算器: <http://localhost:5087/CurrencyConverter>

### 執行測試

```bash
dotnet test
```

## 專案結構

```text
BNICalculate/
├── Pages/
│   ├── BMI.cshtml                  # BMI 計算器頁面
│   ├── BMI.cshtml.cs               # PageModel
│   ├── Pomodoro.cshtml             # 番茄鐘計時器頁面
│   ├── Pomodoro.cshtml.cs          # PageModel
│   ├── WorldClock.cshtml           # 世界時鐘頁面
│   ├── WorldClock.cshtml.cs        # PageModel
│   ├── CurrencyConverter.cshtml    # 匯率計算器頁面
│   └── CurrencyConverter.cshtml.cs # PageModel
├── Models/                         # 資料模型
│   ├── UserSettings.cs             # 使用者設定
│   ├── TimerSession.cs             # 計時時段記錄
│   ├── PomodoroStatistics.cs       # 番茄鐘統計
│   ├── Currency.cs                 # 貨幣資訊
│   ├── ExchangeRate.cs             # 單一匯率
│   ├── ExchangeRateData.cs         # 匯率資料集合
│   ├── CalculationRequest.cs       # 計算請求
│   ├── CalculationResult.cs        # 計算結果
│   ├── TaiwanBankCsvRecord.cs      # 台銀 CSV 記錄
│   ├── DataFormatException.cs      # 資料格式例外
│   └── ExternalServiceException.cs # 外部服務例外
├── Services/                       # 業務邏輯
│   ├── PomodoroDataService.cs      # 番茄鐘 JSON 資料服務
│   ├── ICurrencyService.cs         # 匯率服務介面
│   ├── CurrencyService.cs          # 匯率服務實作
│   ├── ICurrencyDataService.cs     # 匯率資料服務介面
│   └── CurrencyDataService.cs      # 匯率資料服務實作
├── Helpers/
│   └── DateTimeHelper.cs           # 日期時間格式化工具
├── wwwroot/
│   ├── css/
│   │   ├── bmi.css                 # BMI 頁面樣式
│   │   ├── pomodoro.css            # 番茄鐘頁面樣式
│   │   ├── worldclock.css          # 世界時鐘頁面樣式
│   │   └── currency-converter.css  # 匯率計算器頁面樣式
│   └── js/
│       ├── bmi.js                  # BMI 計算邏輯
│       ├── pomodoro.js             # 番茄鐘計時器邏輯
│       ├── worldclock.js           # 世界時鐘邏輯
│       └── currency-converter.js   # 匯率計算器邏輯
├── App_Data/                       # JSON 資料儲存（執行時建立）
│   ├── pomodoro/
│   │   ├── settings.json           # 使用者設定
│   │   └── stats.json              # 今日統計
│   └── currency/
│       └── rates.json              # 匯率資料快取
└── logs/                           # Serilog 日誌（執行時建立）
    └── currency-*.txt              # 每日匯率日誌

BNICalculate.Tests/
├── Unit/                               # 單元測試
│   ├── Helpers/
│   │   └── DateTimeHelperTests.cs     # 日期時間工具測試
│   ├── Models/
│   │   ├── CalculationRequestTests.cs # 計算請求測試
│   │   ├── CalculationResultTests.cs  # 計算結果測試
│   │   ├── ExchangeRateTests.cs       # 匯率測試
│   │   └── ExchangeRateDataTests.cs   # 匯率資料測試
│   └── Services/
│       ├── CurrencyServiceTests.cs    # 匯率服務測試
│       └── CurrencyDataServiceTests.cs # 匯率資料服務測試
├── Integration/                        # 整合測試
│   └── Pages/
│       ├── BMIPageTests.cs            # BMI 整合測試
│       ├── PomodoroPageTests.cs       # 番茄鐘整合測試
│       ├── WorldClockPageTests.cs     # 世界時鐘整合測試
│       └── CurrencyConverterPageTests.cs # 匯率計算器整合測試
├── Services/                           # 服務層測試
│   ├── CurrencyServiceMultiCurrencyTests.cs # 多貨幣測試
│   └── CurrencyServiceRateUpdateTests.cs    # 匯率更新測試
└── Manual/                             # 手動測試
    ├── ConsoleEncodingVerificationTest.cs # 編碼驗證測試
    ├── CurrencyApiEncodingTest.cs     # API 編碼測試
    └── ManualUpdateRatesTest.cs       # 手動更新測試
```

## 程式碼品質

- ✅ 啟用 Nullable 引用型別
- ✅ 將警告視為錯誤
- ✅ XML 文件註解（繁體中文）
- ✅ 遵循 .editorconfig 規範
- ✅ 零警告建置

## 授權

此專案僅供學習和展示用途。
