# BNI Calculate

BMI 計算器與番茄工作法計時器網頁應用程式

## 功能

### BMI 計算器

- **輸入身高和體重**：使用者可輸入身高（公尺）和體重（公斤）
- **即時計算**：點擊「計算」按鈕，系統立即計算 BMI 值並顯示分類
- **WHO 標準分類**：根據 WHO 標準顯示體重分類（過輕、正常、過重、輕度肥胖、中度肥胖、重度肥胖）
- **輸入驗證**：自動驗證輸入資料，顯示清晰的錯誤訊息
- **清除功能**：點擊「清除」按鈕可快速重置所有欄位

### 番茄工作法計時器 (NEW!)

- **25/5 分鐘計時循環**：25 分鐘工作時段 + 5 分鐘休息時段
- **自動階段切換**：工作完成後自動進入休息時段
- **計時器控制**：開始、暫停、繼續、重置功能
- **圓形進度環**：視覺化顯示時段完成進度
- **狀態恢復**：關閉頁面後重新開啟，計時器繼續執行
- **今日統計**：追蹤每日完成的番茄鐘數量
- **自訂時長設定**：調整工作/休息時長（工作：1-60 分鐘，休息：1-30 分鐘）
- **多視窗偵測**：防止同時開啟多個計時器視窗造成衝突
- **Toast 通知**：時段完成時顯示友善提醒

## 技術堆疊

- **後端**: ASP.NET Core 8.0 Razor Pages
- **前端**: 原生 JavaScript + CSS（無框架）
- **資料儲存**: JSON 檔案（App_Data/pomodoro/）
- **快取**: IMemoryCache
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

### 執行測試

```bash
dotnet test
```

## 專案結構

```text
BNICalculate/
├── Pages/
│   ├── BMI.cshtml           # BMI 計算器頁面
│   ├── BMI.cshtml.cs        # PageModel
│   ├── Pomodoro.cshtml      # 番茄鐘計時器頁面
│   └── Pomodoro.cshtml.cs   # PageModel
├── Models/                  # 資料模型
│   ├── UserSettings.cs      # 使用者設定
│   ├── TimerSession.cs      # 計時時段記錄
│   └── PomodoroStatistics.cs # 番茄鐘統計
├── Services/                # 業務邏輯
│   └── PomodoroDataService.cs # JSON 資料服務
├── wwwroot/
│   ├── css/
│   │   ├── bmi.css          # BMI 頁面樣式
│   │   └── pomodoro.css     # 番茄鐘頁面樣式
│   └── js/
│       ├── bmi.js           # BMI 計算邏輯
│       └── pomodoro.js      # 番茄鐘計時器邏輯
└── App_Data/pomodoro/       # JSON 資料儲存（執行時建立）
    ├── settings.json        # 使用者設定
    └── stats.json           # 今日統計

BNICalculate.Tests/
└── Integration/
    └── Pages/
        ├── BMIPageTests.cs      # BMI 整合測試
        └── PomodoroPageTests.cs # 番茄鐘整合測試（可選）
```

## 程式碼品質

- ✅ 啟用 Nullable 引用型別
- ✅ 將警告視為錯誤
- ✅ XML 文件註解（繁體中文）
- ✅ 遵循 .editorconfig 規範
- ✅ 零警告建置

## 授權

此專案僅供學習和展示用途。
