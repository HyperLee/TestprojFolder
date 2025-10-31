# Quick Start Guide: BMI 計算器

**Feature**: 001-bmi-calculator  
**Date**: 2025年10月31日  
**目的**: 開發者快速上手指南

## 概述

本指南說明如何在本地開發環境執行和測試 BMI 計算器功能。

---

## 先決條件

### 必要工具

- ✅ .NET 8.0 SDK ([下載](https://dotnet.microsoft.com/download/dotnet/8.0))
- ✅ Visual Studio Code 或 Visual Studio 2022+
- ✅ Git（用於版本控制）

### 驗證安裝

```bash
# 檢查 .NET 版本
dotnet --version
# 應顯示: 8.0.x
```

---

## 專案設置

### 1. 切換到功能分支

```bash
git checkout 001-bmi-calculator
```

### 2. 還原相依性

```bash
cd BNICalculate
dotnet restore
```

### 3. 建置專案

```bash
dotnet build
```

---

## 開發工作流程

### 啟動開發伺服器

```bash
cd BNICalculate
dotnet run
```

預設會在 <http://localhost:5000> 或 <https://localhost:5001> 啟動。

**存取 BMI 計算器**: 瀏覽 <http://localhost:5000/BMI>

### 熱重載（Hot Reload）

使用 `dotnet watch` 啟用自動重新載入：

```bash
dotnet watch run
```

修改 `.cshtml`、`.cs`、`.css`、`.js` 檔案後，瀏覽器會自動重新整理。

---

## 檔案結構導覽

### 核心檔案

```text
BNICalculate/
├── Pages/
│   ├── BMI.cshtml           # Razor 視圖（HTML 標記）
│   └── BMI.cshtml.cs        # PageModel（C# 後端邏輯）
├── wwwroot/
│   ├── css/
│   │   └── bmi.css          # BMI 頁面樣式
│   └── js/
│       └── bmi.js           # BMI 計算邏輯（JavaScript）
```

### 修改指南

| 要修改... | 編輯檔案 | 說明 |
|-----------|---------|------|
| HTML 結構 | `Pages/BMI.cshtml` | Razor 視圖標記 |
| 頁面後端邏輯 | `Pages/BMI.cshtml.cs` | PageModel（本案例為空，無後端邏輯） |
| 計算邏輯 | `wwwroot/js/bmi.js` | JavaScript 函數 |
| 樣式外觀 | `wwwroot/css/bmi.css` | CSS 規則 |

---

## 測試

### 執行整合測試

```bash
cd BNICalculate.Tests
dotnet test
```

### 測試涵蓋範圍

- ✅ `BMIPageTests.cs`: 驗證頁面載入和 HTML 結構

### 手動測試檢查清單

在瀏覽器中測試以下場景：

#### ✅ 正常流程

1. 輸入身高 1.75 和體重 70 → 點擊「計算」
   - 預期: 顯示「BMI: 22.9」和「分類: 正常」

2. 點擊「清除」
   - 預期: 所有欄位和結果清空

#### ✅ 驗證測試

3. 空值：不輸入任何值 → 點擊「計算」
   - 預期: 顯示「請輸入完整的身高和體重資料」

4. 負數：輸入身高 -1.75 → 點擊「計算」
   - 預期: 顯示「請輸入有效的身高值（大於 0）」

5. 非數字：輸入身高 "abc" → 點擊「計算」
   - 預期: 顯示「請輸入數字」

#### ✅ 邊界測試

6. 過輕：身高 1.60, 體重 45
   - 預期: 「BMI: 17.6」和「分類: 過輕」

7. 中度肥胖：身高 1.70, 體重 90
   - 預期: 「BMI: 31.1」和「分類: 中度肥胖」

---

## 常見問題

### Q: 為什麼修改 JavaScript 後沒有生效？

A: 瀏覽器可能快取了 JS 檔案。嘗試：

1. 硬重新整理（Ctrl+Shift+R 或 Cmd+Shift+R）
2. 清除瀏覽器快取
3. 使用無痕模式測試

### Q: 如何除錯 JavaScript？

A: 使用瀏覽器開發者工具：

1. 按 F12 開啟開發者工具
2. 切換到 Console 標籤查看錯誤
3. 在 Sources 標籤中設置中斷點

### Q: 測試失敗怎麼辦？

A: 檢查：

1. `dotnet --version` 確認 .NET 8.0
2. `dotnet restore` 確保相依性已還原
3. 查看測試輸出的錯誤訊息

---

## 開發最佳實務

### 程式碼風格

- ✅ 遵循 `.editorconfig` 設定
- ✅ C# 命名: PascalCase (public), camelCase (private)
- ✅ JavaScript 命名: camelCase
- ✅ 使用有意義的變數名稱（避免 `x`, `y`, `temp`）

### 提交訊息

遵循 Conventional Commits:

```bash
git commit -m "feat: 新增 BMI 計算器頁面"
git commit -m "style: 調整 BMI 頁面 CSS"
git commit -m "test: 新增 BMI 頁面整合測試"
```

### 程式碼審查

- ✅ 確保所有測試通過
- ✅ 手動測試 5 個核心場景
- ✅ 檢查 lint 錯誤（`dotnet format --verify-no-changes`）

---

## 偵錯技巧

### 後端除錯（C#）

在 Visual Studio Code:

1. 在 `.cs` 檔案中設置中斷點（點擊行號左側）
2. 按 F5 啟動偵錯
3. 瀏覽頁面觸發中斷點

### 前端除錯（JavaScript）

在瀏覽器:

1. F12 → Sources 標籤
2. 找到 `bmi.js`
3. 點擊行號設置中斷點
4. 觸發計算操作

### 常見錯誤

| 錯誤訊息 | 原因 | 解決方案 |
|----------|------|----------|
| `Cannot find module 'bmi.js'` | 路徑錯誤 | 檢查 `<script src="~/js/bmi.js">` |
| `getElementById returns null` | DOM 未載入 | 確保 script 在 `</body>` 前 |
| `dotnet: command not found` | .NET 未安裝 | 安裝 .NET 8.0 SDK |

---

## 效能檢查

### 頁面載入時間

使用 Chrome DevTools:

1. F12 → Network 標籤
2. 勾選「Disable cache」
3. 重新整理頁面（Ctrl+R）
4. 查看「Load」時間（應 <2 秒）

### 計算回應時間

使用 Console:

```javascript
console.time('BMI Calculation');
calculateBMI(); // 你的計算函數
console.timeEnd('BMI Calculation');
// 應 <10ms
```

---

## 部署前檢查清單

- [ ] 所有測試通過（`dotnet test`）
- [ ] 手動測試 7 個場景全部通過
- [ ] 無 console 錯誤或警告
- [ ] 頁面載入時間 <2 秒
- [ ] 計算回應 <1 秒
- [ ] CSS 檔案 <5KB
- [ ] JavaScript 檔案 <10KB
- [ ] 在 Chrome、Firefox、Safari 測試通過
- [ ] 響應式設計測試（手機、平板、桌面）

---

## 參考資源

### 文件連結

- [spec.md](./spec.md) - 功能規格
- [plan.md](./plan.md) - 實作計畫
- [data-model.md](./data-model.md) - 資料模型
- [research.md](./research.md) - 技術決策

### 外部資源

- [ASP.NET Core Razor Pages](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/)
- [JavaScript MDN](https://developer.mozilla.org/en-US/docs/Web/JavaScript)
- [xUnit 文件](https://xunit.net/)

---

## 下一步

完成本地開發設置後：

1. ✅ 閱讀 [spec.md](./spec.md) 了解功能需求
2. ✅ 查看 [data-model.md](./data-model.md) 了解資料結構
3. ✅ 執行 `/speckit.tasks` 生成開發任務清單
4. ✅ 開始 TDD 流程：先寫測試，再實作功能

**準備好了嗎？執行 `/speckit.tasks` 開始編碼！** 🚀
