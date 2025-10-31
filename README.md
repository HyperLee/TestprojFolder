# BNI Calculate

BMI 計算器網頁應用程式

## 功能

### BMI 計算器

- **輸入身高和體重**：使用者可輸入身高（公尺）和體重（公斤）
- **即時計算**：點擊「計算」按鈕，系統立即計算 BMI 值並顯示分類
- **WHO 標準分類**：根據 WHO 標準顯示體重分類（過輕、正常、過重、輕度肥胖、中度肥胖、重度肥胖）
- **輸入驗證**：自動驗證輸入資料，顯示清晰的錯誤訊息
- **清除功能**：點擊「清除」按鈕可快速重置所有欄位

## 技術堆疊

- **後端**: ASP.NET Core 8.0 Razor Pages
- **前端**: 原生 JavaScript + CSS（無框架）
- **測試**: xUnit + WebApplicationFactory

## 快速開始

### 先決條件

- .NET SDK 8.0 或更高版本

### 執行應用程式

```bash
dotnet run --project BNICalculate
```

訪問 <http://localhost:5087/BMI>

### 執行測試

```bash
dotnet test
```

## 專案結構

```text
BNICalculate/
├── Pages/
│   ├── BMI.cshtml           # BMI 計算器頁面
│   └── BMI.cshtml.cs        # PageModel
├── wwwroot/
│   ├── css/bmi.css          # BMI 頁面樣式
│   └── js/bmi.js            # BMI 計算邏輯
└── BNICalculate.csproj

BNICalculate.Tests/
└── Integration/
    └── Pages/
        └── BMIPageTests.cs  # 整合測試
```

## 程式碼品質

- ✅ 啟用 Nullable 引用型別
- ✅ 將警告視為錯誤
- ✅ XML 文件註解（繁體中文）
- ✅ 遵循 .editorconfig 規範
- ✅ 零警告建置

## 效能指標

- 頁面載入時間: < 2 秒
- BMI 計算回應時間: < 1 秒
- TTI (Time to Interactive): < 3 秒
- CSS 檔案大小: 1.35 KB
- JavaScript 檔案大小: 5.21 KB

## 授權

此專案僅供學習和展示用途。
