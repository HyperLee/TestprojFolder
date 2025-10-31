# Implementation Plan: 世界時鐘 (World Clock)

**Branch**: `003-world-clock` | **Date**: 2025-11-01 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/003-world-clock/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

建立一個世界時鐘功能，讓使用者能夠同時查看 10 個主要城市（台北、東京、倫敦、紐約、洛杉磯、巴黎、柏林、莫斯科、新加坡、悉尼）的即時時間。系統將使用 ASP.NET Core 8.0 Razor Pages 實作，採用客戶端 JavaScript 進行即時時間更新，並提供互動式的時區切換功能。主要時間顯示於頁面中央，其他城市以網格方式排列。使用者可點選任何城市時間將其設為主要顯示，系統自動處理夏令時間轉換和日期變更。

## Technical Context

**Language/Version**: C# 12 / .NET 8.0  
**Primary Dependencies**: ASP.NET Core 8.0 Razor Pages, Bootstrap 5 (已包含在現有專案中)  
**Storage**: 不需要（純客戶端時間計算）  
**Testing**: xUnit + WebApplicationFactory (整合測試)  
**Target Platform**: Web (跨瀏覽器：Chrome、Firefox、Safari、Edge)  
**Project Type**: Web (Razor Pages) - 現有 BNICalculate 專案擴充  
**Performance Goals**:

- 頁面載入時間 < 2 秒
- 時間更新延遲 < 100 毫秒
- 視覺回饋響應 < 50 毫秒
- 支援長時間運行（24 小時無誤差累積）

**Constraints**:

- 必須整合到現有 BNICalculate 專案中
- 使用現有的 Bootstrap 5 和 jQuery 基礎設施
- 純客戶端時間計算（使用 JavaScript Date API 和 Intl.DateTimeFormat）
- 支援響應式設計（行動裝置、平板、桌面）
- 無障礙設計符合 WCAG 2.1 Level AA
- 所有城市時間必須正確處理夏令時間

**Scale/Scope**:

- 單一頁面功能
- 10 個預設城市時區
- 無伺服器端資料存取需求
- 預期使用者負載：與現有 BMI 和 Pomodoro 功能相同

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### I. 程式碼品質標準

- ✅ **靜態分析**: 將啟用 `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` 和 `<Nullable>enable</Nullable>`
- ✅ **程式碼風格**: 遵循 `.editorconfig` 定義的 C# 命名慣例（PascalCase for public, camelCase for private）
- ✅ **SOLID 原則**: PageModel 類別遵循單一職責原則，無需額外的相依性注入（純客戶端邏輯）
- ✅ **文件撰寫**: PageModel 類別將使用繁體中文 XML 文件註解
- ✅ **程式碼審查**: 所有變更透過 Pull Request 進行審查

### II. 測試標準

- ✅ **TDD 工作流程**: 先撰寫整合測試（WorldClockPageTests），再實作頁面
- ✅ **測試架構**: 使用 xUnit + WebApplicationFactory 進行整合測試
- ✅ **覆蓋率要求**: 目標 80% 覆蓋率（PageModel 和 JavaScript 互動）
- ✅ **測試類型**:
  - 整合測試：WorldClockPageTests.cs（驗證頁面載入和 HTML 結構）
  - UI 測試：使用 Selenium/Playwright 驗證時間更新和互動功能（如適用）
- ✅ **測試品質**: 測試獨立、可重複、快速（整合測試 <5 秒）

### III. 使用者體驗一致性

- ✅ **設計系統**: 使用現有的 Bootstrap 5 和 `_Layout.cshtml`
- ✅ **響應式設計**: 行動優先，支援手機、平板、桌面
- ✅ **無障礙設計**: 符合 WCAG 2.1 Level AA（語意化 HTML、ARIA 標籤、鍵盤導覽）
- ✅ **錯誤處理**: 時區載入失敗時顯示使用者友善的錯誤訊息
- ✅ **載入狀態**: 頁面載入時顯示載入指示器（如需要）
- ✅ **一致性**: 遵循現有 BMI 和 Pomodoro 頁面的設計模式

### IV. 效能要求

- ✅ **頁面載入效能**: 初始載入 <2 秒（符合規範）
- ✅ **API 回應時間**: 不適用（無伺服器端 API）
- ✅ **資料存取最佳化**: 不適用（純客戶端計算）
- ✅ **記憶體管理**: JavaScript 計時器正確清理（避免記憶體洩漏）
- ✅ **客戶端資源**: JavaScript <50KB、CSS <20KB（估計值）
- ✅ **ASP.NET Core 優化**: 使用 Response Caching 快取靜態頁面

### V. 文件與溝通語言

- ✅ **功能規範**: spec.md 使用繁體中文
- ✅ **實作計畫**: plan.md 使用繁體中文
- ✅ **使用者介面**: 所有 UI 文字使用繁體中文
- ✅ **程式碼註解**: XML 文件註解使用繁體中文

**憲章合規性評估**: ✅ **通過** - 此功能完全符合專案憲章的所有核心原則，無需例外處理。

## Project Structure

### Documentation (this feature)

```text
specs/003-world-clock/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   ├── README.md
│   └── world-clock-data.md
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
BNICalculate/                        # 現有專案根目錄
├── Pages/
│   ├── WorldClock.cshtml            # 世界時鐘 Razor 頁面（新增）
│   ├── WorldClock.cshtml.cs         # WorldClock PageModel（新增）
│   ├── _ViewImports.cshtml          # 現有
│   └── Shared/
│       └── _Layout.cshtml           # 現有（可能需要新增導覽連結）
├── wwwroot/
│   ├── css/
│   │   └── worldclock.css           # 世界時鐘樣式（新增）
│   ├── js/
│   │   └── worldclock.js            # 世界時鐘邏輯（新增）
│   └── lib/                         # 現有（Bootstrap 5、jQuery）
└── Program.cs                       # 現有（可能無需修改）

BNICalculate.Tests/                  # 現有測試專案
├── Integration/
│   └── Pages/
│       └── WorldClockPageTests.cs   # 整合測試（新增）
└── BNICalculate.Tests.csproj        # 現有
```

**Structure Decision**: 採用現有 ASP.NET Core Razor Pages 專案結構。世界時鐘功能將作為新的 Razor 頁面（WorldClock.cshtml）整合到 `BNICalculate` 專案中，遵循與現有 BMI 和 Pomodoro 頁面相同的模式。主要邏輯將在客戶端 JavaScript（worldclock.js）中實作，PageModel 僅負責提供基本的頁面模型支援。測試將放置在現有的 `BNICalculate.Tests` 專案中，遵循與 BMIPageTests 和 PomodoroPageTests 相同的結構。

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

無違規 - 此功能完全符合專案憲章的所有要求，無需複雜性追蹤。

---

## Phase 0: Research ✅ COMPLETED

**目標**: 研究並確定技術實作方案

**完成時間**: 2025-11-01

**產出**:

- ✅ [research.md](research.md) - 技術研究文件

**關鍵決策**:

1. **時區處理**: 使用原生 `Intl.DateTimeFormat` API（無需第三方函式庫）
2. **更新機制**: 使用 `setInterval` + 準確時間戳計算
3. **無障礙設計**: ARIA Live Regions + 語意化 HTML
4. **效能優化**: Formatter 實例重用 + Page Visibility API
5. **視覺設計**: CSS Grid + CSS Variables + Bootstrap 5 整合

**所有 NEEDS CLARIFICATION 已解決** ✅

---

## Phase 1: Design & Contracts ✅ COMPLETED

**目標**: 定義資料模型和 API 合約

**完成時間**: 2025-11-01

**產出**:

- ✅ [data-model.md](data-model.md) - 資料模型定義
- ✅ [contracts/world-clock-data.md](contracts/world-clock-data.md) - 資料結構合約
- ✅ [contracts/README.md](contracts/README.md) - 合約文件索引
- ✅ [quickstart.md](quickstart.md) - 快速開發指南
- ✅ `.github/copilot-instructions.md` - 更新 GitHub Copilot 上下文

**核心實體**:

1. **CityTimezone**: 城市時區配置（10 個預設城市）
2. **ClockState**: 時鐘狀態管理（主要城市 + 9 個次要城市）
3. **FormattedTime**: 格式化時間輸出（運行時計算）

**資料流程**:

- 初始化流程：載入配置 → 建立 Formatter → 啟動時鐘
- 更新流程：每秒觸發 → 計算時間 → 更新 DOM
- 切換流程：點選城市 → 交換主次城市 → 更新 UI
- 清理流程：頁面卸載 → 停止計時器 → 釋放資源

**Constitution Check 重新驗證**: ✅ 通過

所有設計決策符合專案憲章，無需例外處理或複雜性追蹤。

---

## Phase 2: Task Breakdown

**注意**: Phase 2（任務分解）由 `/speckit.tasks` 命令產生，不包含在此計畫文件中。

Phase 1 完成後，執行以下命令進入 Phase 2：

```bash
# 產生任務清單
/speckit.tasks
```

這將產生 `specs/003-world-clock/tasks.md` 檔案，包含：

- TDD 工作流程的詳細任務分解
- 每個任務的驗收標準
- 測試優先的實作順序
- 時間估算和優先級

---

## 實作就緒檢查清單

在開始實作前，確認以下項目：

- ✅ 功能規格已審查並核准（[spec.md](spec.md)）
- ✅ 技術研究已完成（[research.md](research.md)）
- ✅ 資料模型已定義（[data-model.md](data-model.md)）
- ✅ 合約已記錄（[contracts/](contracts/)）
- ✅ 快速開發指南已準備（[quickstart.md](quickstart.md)）
- ✅ 憲章合規性已驗證（Constitution Check 通過）
- ✅ Agent 上下文已更新（copilot-instructions.md）
- ⏳ 任務清單待產生（執行 `/speckit.tasks`）

**實作可以開始** - 所有計畫階段文件已完成 ✅

---

**計畫完成日期**: 2025-11-01  
**下一步**: 執行 `/speckit.tasks` 產生詳細任務分解
