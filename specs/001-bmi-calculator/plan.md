# Implementation Plan: BMI 計算器網頁

**Branch**: `001-bmi-calculator` | **Date**: 2025年10月31日 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-bmi-calculator/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

實作一個超簡易的 BMI 計算器網頁，使用 ASP.NET Core 8.0 Razor Pages。使用者輸入身高（公尺）和體重（公斤），系統使用客戶端 JavaScript 計算 BMI 值並顯示 WHO 標準的體重分類。介面簡潔樸素，無需資料庫或前端框架，所有計算在瀏覽器端完成。

## Technical Context

**Language/Version**: C# 11 / .NET 8.0  
**Primary Dependencies**: ASP.NET Core 8.0 Razor Pages (無額外 NuGet 套件)  
**Storage**: N/A (無資料持久化需求)  
**Testing**: xUnit + WebApplicationFactory (整合測試)  
**Target Platform**: Web (支援現代瀏覽器 Chrome/Firefox/Safari/Edge)  
**Project Type**: Web (Razor Pages with minimal JavaScript)  
**Performance Goals**:

- 頁面載入時間 <2 秒
- BMI 計算回應時間 <1 秒 (客戶端即時計算)
- TTI <3 秒

**Constraints**:

- 無資料庫、無後端 API
- 純客戶端計算（JavaScript）
- 簡潔樸素 UI（無特效、無 CSS 框架如 Bootstrap）
- 使用 Razor Pages 內建功能

**Scale/Scope**:

- 單一頁面應用
- 無使用者認證
- 無狀態管理需求
- 無並發使用者限制（靜態內容）

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### I. 程式碼品質標準

- ✅ **靜態分析**: 將啟用 `<Nullable>enable</Nullable>` 和 `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
- ✅ **程式碼風格**: 使用既有 `.editorconfig`，遵循 C# 命名慣例
- ✅ **SOLID 原則**: 不需要 DI（無服務層），所有邏輯在客戶端 JavaScript
- ✅ **文件撰寫**: Razor Page 的 PageModel 將使用 XML 註解（繁體中文）
- ✅ **程式碼審查**: 所有變更通過 PR 審查

**合規性**: ✅ PASS - 簡單功能無複雜架構需求

### II. 測試標準

- ✅ **TDD 工作流程**: 先寫整合測試（Razor Page 渲染測試）
- ✅ **測試架構**: 使用 xUnit + WebApplicationFactory
- ⚠️ **覆蓋率要求**: JavaScript 邏輯使用 Jest 或手動測試
  - Razor Page: 整合測試（頁面載入、HTML 結構驗證）
  - JavaScript: 瀏覽器測試或 Jest 單元測試（BMI 計算邏輯）
- ✅ **測試品質**: 獨立、可重複、快速

**合規性**: ✅ PASS - 將建立測試專案

### III. 使用者體驗一致性

- ✅ **設計系統**: 使用自訂 CSS（site.css），無 Bootstrap
- ✅ **響應式設計**: 使用基本 CSS media queries
- ✅ **無障礙設計**: 使用語意化 HTML，支援鍵盤導覽
- ✅ **錯誤處理**: 客戶端驗證，欄位下方顯示錯誤訊息
- ✅ **一致性**: 單一頁面，無需跨頁面一致性

**合規性**: ✅ PASS - 符合簡潔樸素原則

### IV. 效能要求

- ✅ **頁面載入**: 目標 <2 秒（靜態 HTML + 小型 JS）
- ✅ **TTI**: 目標 <3 秒
- ✅ **API 回應**: N/A（無後端 API）
- ✅ **客戶端資源**: JS <10KB, CSS <5KB（無框架）
- ✅ **ASP.NET 最佳化**: 啟用靜態檔案壓縮

**合規性**: ✅ PASS - 簡單頁面易達成目標

### V. 文件與溝通語言

- ✅ **功能規範**: spec.md 使用繁體中文 ✓
- ✅ **實作計畫**: plan.md 使用繁體中文 ✓
- ✅ **使用者介面**: UI 文字使用繁體中文 ✓
- ✅ **註解**: XML 文件註解使用繁體中文 ✓

**合規性**: ✅ PASS - 完全符合

### 總結

**整體合規性**: ✅ **PASS** - 所有憲章要求符合，無違規項目。此功能範圍簡單，無複雜架構或效能挑戰。

## Project Structure

### Documentation (this feature)

```text
specs/001-bmi-calculator/
├── plan.md              # ✅ 本檔案（實作計畫）
├── research.md          # ✅ Phase 0 完成（技術決策）
├── data-model.md        # ✅ Phase 1 完成（資料模型）
├── quickstart.md        # ✅ Phase 1 完成（快速開始）
├── contracts/           # ✅ Phase 1 完成（無 API，目錄為空）
│   └── README.md
├── spec.md              # ✅ 功能規格（已完成）
└── tasks.md             # ⏳ Phase 2（待執行 /speckit.tasks）
```
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
BNICalculate/                          # 主要專案目錄（現有）
├── Pages/
│   ├── BMI.cshtml                     # 新增：BMI 計算器頁面（Razor 視圖）
│   ├── BMI.cshtml.cs                  # 新增：BMI 頁面 PageModel（C# 後端）
│   ├── Shared/
│   │   └── _Layout.cshtml             # 現有：共用版面配置
│   ├── _ViewImports.cshtml            # 現有：視圖導入
│   └── _ViewStart.cshtml              # 現有：視圖啟動
├── wwwroot/
│   ├── css/
│   │   └── bmi.css                    # 新增：BMI 頁面專用樣式
│   ├── js/
│   │   └── bmi.js                     # 新增：BMI 計算邏輯（JavaScript）
│   └── lib/                           # 現有：第三方函式庫
├── Program.cs                         # 現有：應用程式進入點
├── appsettings.json                   # 現有：設定檔
└── BNICalculate.csproj                # 現有：專案檔

BNICalculate.Tests/                    # 新增：測試專案
├── Integration/
│   └── Pages/
│       └── BMIPageTests.cs            # 新增：BMI 頁面整合測試
├── BNICalculate.Tests.csproj          # 新增：測試專案檔
└── Usings.cs                          # 新增：全域 using 宣告

specs/001-bmi-calculator/              # 規格與計畫文件
├── spec.md                            # 功能規格
├── plan.md                            # 本檔案（實作計畫）
├── research.md                        # Phase 0：研究決策
├── data-model.md                      # Phase 1：資料模型
├── quickstart.md                      # Phase 1：快速開始指南
└── contracts/                         # Phase 1：API 合約（本案例為空，無後端 API）
```

**Structure Decision**: 使用 ASP.NET Core Razor Pages 標準結構。新增單一 BMI 頁面（`BMI.cshtml` + `BMI.cshtml.cs`），配套的 JavaScript（`bmi.js`）和 CSS（`bmi.css`）檔案。建立獨立測試專案 `BNICalculate.Tests` 進行整合測試。無需 API 層或資料存取層，所有計算在客戶端完成。

## Complexity Tracking

**N/A** - 無憲章違規項目，無需複雜度追蹤。

---

## Phase Summary

### ✅ Phase 0: Research & Decisions (完成)

產出: `research.md`

關鍵決策:

1. 客戶端計算（JavaScript）
2. JavaScript 驗證 + HTML5 輔助屬性
3. 自訂 CSS（無框架）
4. 整合測試（xUnit）
5. 路徑 `/BMI`

### ✅ Phase 1: Design & Contracts (完成)

產出:

- `data-model.md` - 定義 3 個 JavaScript 物件（UserInput, BMIResult, ValidationError）
- `quickstart.md` - 開發者快速上手指南
- `contracts/README.md` - 說明無後端 API

設計決策:

- 無資料持久化
- 無後端邏輯（PageModel 為空）
- 純客戶端狀態管理

### ⏳ Phase 2: Task Breakdown (待執行)

**下一步**: 執行 `/speckit.tasks` 生成開發任務清單（tasks.md）

---

## Implementation Readiness

### ✅ 準備就緒項目

- [x] 技術堆疊確定（ASP.NET Core 8.0 + Vanilla JS）
- [x] 架構設計完成（Razor Pages + 客戶端計算）
- [x] 資料模型定義（JavaScript 物件）
- [x] 驗證邏輯設計（JavaScript 函數）
- [x] 測試策略確定（xUnit 整合測試）
- [x] 開發環境文件（quickstart.md）
- [x] 憲章合規檢查通過

### 📋 等待執行

- [ ] 建立測試專案（`BNICalculate.Tests`）
- [ ] 撰寫整合測試（TDD: Red → Green → Refactor）
- [ ] 實作 Razor Page（`BMI.cshtml` + `BMI.cshtml.cs`）
- [ ] 實作 JavaScript 邏輯（`bmi.js`）
- [ ] 實作 CSS 樣式（`bmi.css`）
- [ ] 執行手動測試驗證
- [ ] 效能驗證（<2秒載入，<1秒計算）

---

## 風險評估

| 風險 | 影響 | 機率 | 緩解措施 | 狀態 |
|------|------|------|----------|------|
| JavaScript 瀏覽器相容性 | 中 | 低 | 使用 ES5 語法，避免 ES6+ | ✅ 已緩解 |
| 測試覆蓋率不足 | 中 | 低 | 整合測試 + 手動測試清單 | ✅ 已規劃 |
| 效能未達標 | 低 | 極低 | 簡單頁面，無複雜運算 | ✅ 風險低 |
| 使用者停用 JS | 低 | 低 | 顯示提示訊息 | ✅ 已規劃 |

---

## Next Steps

1. **執行** `/speckit.tasks` **產生任務清單**
2. 開始 TDD 工作流程：
   - 撰寫失敗測試
   - 實作功能
   - 測試通過
   - 重構
3. 遵循 `quickstart.md` 的開發指南
4. 完成後執行完整測試套件和手動驗證

**準備好開始編碼了嗎？執行 `/speckit.tasks` 🚀**

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
