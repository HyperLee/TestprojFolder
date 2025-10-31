# Implementation Plan: 番茄工作法計時器

**Branch**: `002-pomodoro-timer` | **Date**: 2025-10-31 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-pomodoro-timer/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

實作一個整合至現有 ASP.NET Core Razor Pages 專案的番茄工作法計時器頁面。系統提供 25/5 分鐘的工作/休息循環計時，使用客戶端 JavaScript 進行即時倒數，並透過本地 JSON 檔案儲存使用者設定和每日統計資料。核心功能包含計時器控制（開始/暫停/繼續/重置）、圓形進度環視覺化、番茄鐘計數追蹤、自訂時長設定，以及多視窗衝突偵測。技術方法採用極簡架構：單一 Razor Page + PageModel + 檔案操作服務 + Vanilla JavaScript，無需額外專案或複雜框架。

## Technical Context

**Language/Version**: C# 12 / .NET 8.0  
**Primary Dependencies**: ASP.NET Core 8.0 Razor Pages（現有專案），System.Text.Json  
**Storage**: JSON 檔案儲存（wwwroot/data/ 或 App_Data/），無資料庫  
**Testing**: xUnit + WebApplicationFactory（整合測試），Playwright（UI 測試）  
**Target Platform**: 桌面瀏覽器（Chrome, Edge, Firefox, Safari），Windows/macOS/Linux 伺服器  
**Project Type**: Web Application（現有 BNICalculate 專案擴充，非新專案）  
**Performance Goals**: 頁面載入 <500ms，計時器更新延遲 <50ms，JSON 讀寫 <100ms  
**Constraints**: 僅桌面瀏覽器（無行動裝置），單一使用者，離線可用，無外部 API，時間準確度 ±1 秒  
**Scale/Scope**: 單頁面（Pomodoro.cshtml），4 個實體類別，2 個服務類別，約 300-400 行 C# + 200-300 行 JavaScript

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### 檢查結果

| 憲章原則 | 狀態 | 說明 |
|---------|------|------|
| **I. 程式碼品質標準** | ✅ PASS | 遵循現有專案的 .editorconfig，啟用 Nullable 參考型別，使用 async/await，XML 文件註解繁體中文 |
| **II. 測試標準** | ✅ PASS | 使用 xUnit（現有測試專案），目標覆蓋率：服務層 80%+，關鍵路徑（計時邏輯）100% |
| **III. UX 一致性** | ✅ PASS | 整合現有 _Layout.cshtml，使用共享 CSS/JS，橫幅通知統一樣式，錯誤處理友善訊息 |
| **IV. 效能要求** | ✅ PASS | JSON 檔案非同步讀寫，IMemoryCache 快取，客戶端 JavaScript <50KB，圓形進度環使用 CSS/SVG 而非重型函式庫 |
| **V. 文件語言** | ✅ PASS | 所有文件（spec, plan, tasks）繁體中文，UI 文字繁體中文，XML 註解繁體中文 |

### 複雜度評估（憲章 Gate 0 檢查）

**專案數量**: ✅ 符合 - 使用現有 BNICalculate 專案（共 1 個主專案 + 1 個測試專案，未超過 3 個限制）

**架構模式**: ✅ 符合 - 不使用 Repository pattern，直接使用檔案操作服務（PomodoroDataService）

**ORM/資料層**: ✅ 符合 - 僅使用 System.Text.Json 序列化，無 Entity Framework 或複雜 ORM

**前端框架**: ✅ 符合 - Vanilla JavaScript + Razor Pages，無 React/Vue/Angular

**外部依賴**: ✅ 符合 - 僅依賴 .NET 內建函式庫（System.Text.Json, System.IO, IMemoryCache）

**結論**: 無需填寫 Complexity Tracking 表格，所有設計決策符合憲章極簡原則。

## Project Structure

### Documentation (this feature)

```text
specs/002-pomodoro-timer/
├── spec.md              # 功能規格（已完成）
├── plan.md              # 本檔案（實作計畫）
├── research.md          # Phase 0 輸出（技術研究）
├── data-model.md        # Phase 1 輸出（資料模型）
├── quickstart.md        # Phase 1 輸出（快速開始指南）
├── contracts/           # Phase 1 輸出（API 合約，本專案為客戶端 API）
│   └── pomodoro-api.md  # JavaScript API 規格
├── checklists/          # 品質檢查清單
│   └── requirements.md  # 需求完整性檢查（已完成）
└── tasks.md             # Phase 2 輸出（任務清單，由 /speckit.tasks 生成）
```

### Source Code (整合至現有專案)

```text
BNICalculate/  （現有專案根目錄）
├── Pages/
│   ├── Pomodoro.cshtml          # 新增：番茄鐘計時器頁面（Razor 視圖）
│   ├── Pomodoro.cshtml.cs       # 新增：PageModel（伺服器端邏輯）
│   └── Shared/
│       └── _Layout.cshtml       # 修改：新增導覽連結
├── Models/  （新增資料夾）
│   ├── TimerSession.cs          # 新增：計時器時段實體
│   ├── PomodoroStatistics.cs   # 新增：番茄鐘統計實體
│   ├── UserSettings.cs          # 新增：使用者設定實體
│   └── TimerState.cs            # 新增：計時器狀態實體
├── Services/  （新增資料夾）
│   ├── PomodoroDataService.cs   # 新增：JSON 檔案操作服務
│   └── TimerValidationService.cs # 新增：輸入驗證服務
├── wwwroot/
│   ├── js/
│   │   └── pomodoro.js          # 新增：客戶端計時器邏輯
│   ├── css/
│   │   └── pomodoro.css         # 新增：番茄鐘頁面樣式
│   └── data/  （新增資料夾，可選用 App_Data）
│       ├── pomodoro-settings.json  # 執行時建立：使用者設定
│       └── pomodoro-stats.json     # 執行時建立：統計資料
├── Program.cs                   # 修改：註冊服務（DI）
└── BNICalculate.csproj          # 無需修改（使用現有相依性）

BNICalculate.Tests/  （現有測試專案）
├── Unit/
│   ├── Services/
│   │   ├── PomodoroDataServiceTests.cs  # 新增：資料服務單元測試
│   │   └── TimerValidationServiceTests.cs # 新增：驗證服務單元測試
│   └── Models/
│       └── TimerSessionTests.cs         # 新增：實體邏輯測試（如有）
├── Integration/
│   └── Pages/
│       └── PomodoroPageTests.cs         # 新增：PageModel 整合測試
└── UI/  （可選，視資源而定）
    └── PomodoroUITests.cs               # 新增：Playwright UI 測試
```

**Structure Decision**: 採用 ASP.NET Core Razor Pages 標準結構，將新功能整合至現有 BNICalculate 專案。選擇此結構的理由：

1. **最小侵入性**：僅新增頁面和支援檔案，不改變專案架構
2. **符合現有慣例**：遵循現有 BMI 頁面（BMI.cshtml）的模式
3. **清晰分層**：Pages（展示層）、Models（資料層）、Services（業務邏輯層）分離
4. **可測試性**：服務層和 PageModel 可獨立測試
5. **無額外專案**：避免違反憲章「不超過 3 個專案」原則

## Complexity Tracking

> 本專案無憲章違規，無需填寫此表格。所有設計決策符合極簡原則。

---

## Phase 0: 研究與決策記錄

### 目標

解決技術背景中的所有不確定性，為 Phase 1 設計提供技術決策依據。

### 產出文件

- ✅ `research.md` - 技術研究文件（已完成）

### 關鍵研究主題

已完成以下 8 個技術領域的研究：

1. **R1: JSON 檔案儲存最佳實務**
   - 決策：System.Text.Json + 非同步 I/O
   - 儲存位置：App_Data/pomodoro/
   - 理由：高效能、零依賴、安全性

2. **R2: 客戶端計時器實作與時間準確性**
   - 決策：setInterval + Date.now() 校準機制
   - 準確度：±1 秒（符合 SC-002）
   - 支援：頁面離開/返回狀態恢復

3. **R3: 多視窗衝突偵測機制**
   - 決策：localStorage + 心跳機制 + storage 事件
   - 容錯：5 秒心跳超時，自動接管
   - 使用者體驗：顯示警告橫幅並禁用功能

4. **R4: 圓形進度環（Circular Progress Ring）實作**
   - 決策：SVG + CSS 動畫
   - 技術：stroke-dasharray/offset 控制進度
   - 理由：零依賴、硬體加速、可縮放

5. **R5: 跨日界計時處理邏輯**
   - 決策：工作開始日期歸屬（23:50 開始計入當天）
   - 自動重置：午夜重新載入頁面時重置計數
   - 符合：Clarification #3 使用者預期

6. **R6: 橫幅通知實作**
   - 決策：Bootstrap Toast 元件
   - 自動消失：4 秒（符合 Clarification #1）
   - 類型：success/info/warning/danger 5 種訊息

7. **R7: 輸入驗證與錯誤處理**
   - 決策：HTML5 前端驗證 + Data Annotations 後端驗證
   - 範圍：工作 1-60 分鐘，休息 1-30 分鐘
   - 雙重防護：使用者體驗 + 安全性

8. **R8: 記憶體快取策略**
   - 決策：IMemoryCache 快取設定（10 分鐘滑動過期）
   - 統計資料不快取（即時更新）
   - 效能：減少檔案 I/O

### 技術決策摘要表

| 決策領域 | 選定技術 | 關鍵理由 |
|---------|---------|---------|
| JSON 序列化 | System.Text.Json | 高效能、零依賴、.NET 內建 |
| 檔案儲存位置 | App_Data/ | 安全性（IIS 拒絕直接存取） |
| 客戶端計時器 | setInterval + Date.now() 校準 | 時間準確性、簡單可靠 |
| 多視窗偵測 | localStorage + 心跳機制 | 即時偵測、容錯性高 |
| 進度視覺化 | SVG + CSS 動畫 | 零依賴、效能優異、可縮放 |
| 通知實作 | Bootstrap Toast | 現有依賴、無障礙、一致性 |
| 輸入驗證 | HTML5 + Data Annotations | 使用者體驗 + 安全性 |
| 快取機制 | IMemoryCache | 減少 I/O、內建支援 |

### 風險評估

| 風險 | 影響 | 緩解措施 | 狀態 |
|-----|------|---------|------|
| JSON 併發寫入衝突 | 中 | 單使用者場景，風險極低 | ✅ 可接受 |
| localStorage 限制 | 低 | 資料量 <10KB，遠低於 5MB 限制 | ✅ 無風險 |
| 背景計時器不準確 | 中 | Date.now() 校準，誤差 <1 秒 | ✅ 已解決 |
| SVG 瀏覽器相容性 | 低 | 所有現代瀏覽器支援 | ✅ 無風險 |

### 驗證清單

- [x] 所有 `NEEDS CLARIFICATION` 標記已解決
- [x] 技術選型符合憲章極簡原則（無過度工程化）
- [x] 替代方案已評估並記錄理由
- [x] 實作模式包含程式碼範例
- [x] 風險已識別且有緩解措施
- [x] 符合 .NET 8.0 最佳實務

---

## Phase 1: 設計與合約

### Phase 1 目標

基於 Phase 0 研究結果，設計資料模型、API 合約和開發者文件。

### Phase 1 產出文件

- ✅ `data-model.md` - 資料實體設計（已完成）
- ✅ `contracts/README.md` - API 合約總覽（已完成）
- ✅ `contracts/pomodoro-api.md` - JavaScript 客戶端 API 規格（已完成）
- ✅ `quickstart.md` - 開發者快速上手指南（已完成）

### Phase 1 完成摘要

已完成以下設計文件：

1. **data-model.md** - 定義 4 個核心實體：
   - E1: UserSettings（使用者設定）- 工作/休息時長、音效偏好
   - E2: TimerSession（計時器時段記錄）- 每次完成的工作/休息時段
   - E3: PomodoroStatistics（番茄鐘統計）- 每日完成數量、總時長
   - E4: TimerState（計時器狀態）- 客戶端 localStorage 狀態

2. **contracts/pomodoro-api.md** - 定義 3 個 JavaScript API：
   - API-1: PomodoroTimer 類別（核心計時器邏輯）
   - API-2: MultiWindowGuard 類別（多視窗衝突偵測）
   - API-3: NotificationUI 工具函式（Toast 通知管理）

3. **quickstart.md** - 開發者指南包含：
   - 環境設定（.NET 8.0 SDK, 相依套件還原）
   - 本機執行步驟（dotnet run, 瀏覽至 /Pomodoro）
   - 測試執行（dotnet test, 覆蓋率報告）
   - 偵錯技巧（C# 中斷點, JavaScript Console）
   - 常見問題排解（5 個常見場景）

---

## Phase 2: 任務分解

### Phase 2 目標

將功能規格拆解為可測試的獨立任務，依使用者故事（User Story）組織，確保漸進式交付。

### Phase 2 產出文件

- ✅ `tasks.md` - 實作任務清單（已完成）

### Phase 2 完成摘要

已完成任務分解文件：

1. **tasks.md** - 包含 119 個任務：
   - Phase 1: Setup（5 個任務）- 專案初始化、目錄建立
   - Phase 2: Foundational（10 個任務）- 基礎設施、模型、服務
   - Phase 3: User Story 1（26 個任務）- 基本番茄鐘工作循環（P1 - MVP）
   - Phase 4: User Story 2（13 個任務）- 計時器暫停與重置控制（P2）
   - Phase 5: User Story 3（21 個任務）- 番茄鐘統計追蹤（P2）
   - Phase 6: User Story 4（15 個任務）- 自訂時長設定（P3）
   - Phase 7: User Story 5（8 個任務）- 視覺化進度顯示（P3）
   - Phase 8: Polish（21 個任務）- 跨故事改進、文件、驗證

2. **測試環境備案**：
   - 所有測試任務標記為 OPTIONAL
   - 包含測試環境驗證任務（T015）
   - 若測試環境有問題（建置失敗、執行卡住），可跳過測試任務
   - 提供手動測試替代方案（每個 User Story 包含 Independent Test）

3. **任務組織方式**：
   - 依使用者故事分組，確保每個故事可獨立測試
   - 標記 [P] 可平行執行任務（約 40 個）
   - 所有任務包含明確檔案路徑
   - 格式：`- [ ] [TaskID] [P?] [Story?] Description with file path`

4. **實作策略**：
   - **MVP First**: Phase 1 + 2 + 3（User Story 1）完成後即可展示
   - **Incremental Delivery**: 每個 User Story 完成後可獨立部署
   - **Parallel Team Strategy**: Foundational 完成後，所有 User Story 可平行開發

---

## 進度追蹤

| 階段 | 狀態 | 完成日期 |
|-----|------|---------|
| Phase 0: 研究 | ✅ 完成 | 2025-10-31 |
| Phase 1: 設計 | ✅ 完成 | 2025-10-31 |
| Phase 2: 任務 | ✅ 完成 | 2025-10-31 |

---

## 總結

**Phase 0 + Phase 1 + Phase 2 已完成**，所有設計文件和任務清單已就緒：

- ✅ research.md - 8 個技術決策（JSON 儲存、計時器、多視窗、進度環等）
- ✅ data-model.md - 4 個實體設計（UserSettings, TimerSession, PomodoroStatistics, TimerState）
- ✅ contracts/pomodoro-api.md - 3 個 JavaScript API（PomodoroTimer, MultiWindowGuard, NotificationUI）
- ✅ quickstart.md - 完整開發者指南（環境設定、測試、偵錯、排解問題）
- ✅ tasks.md - 119 個可執行任務（包含測試環境備案）

**下一步**: 開始實作任務，建議從 MVP（Phase 1 + 2 + 3）開始，完成基本番茄鐘工作循環。
