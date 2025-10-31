# Tasks: BMI 計算器網頁

**Feature**: 001-bmi-calculator  
**Branch**: `001-bmi-calculator`  
**Input**: 設計文件來自 `/specs/001-bmi-calculator/`  
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, quickstart.md ✅

**Tests**: 此專案採用 TDD 方法，包含整合測試任務。

**Organization**: 任務按使用者故事分組，使每個故事可獨立實作和測試。

## Format: `[ID] [Story] Description`

- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

本專案使用 ASP.NET Core Razor Pages 標準結構：

- **主專案**: `BNICalculate/`
- **測試專案**: `BNICalculate.Tests/`
- **文件**: `specs/001-bmi-calculator/`

---

## Phase 1: Setup (共用基礎設施)

**目的**: 建立測試專案和基礎設定

### 環境驗證 ⚠️ 必須先執行

- [ ] T001 驗證 .NET SDK 版本（執行 `dotnet --version`，確認 >= 8.0）
- [ ] T002 驗證主專案可正常建置（執行 `dotnet build BNICalculate/BNICalculate.csproj`，確認無錯誤）
- [ ] T003 驗證主專案可正常執行（執行 `dotnet run --project BNICalculate`，確認端口 5000 可用）

**預期時間**: 10 分鐘

**⚠️ 檢查點**: 如環境驗證失敗，記錄問題並**跳過所有測試任務**，改用手動測試（見備案方案）

### 測試專案建置（可選）

> **備案**: 如果測試專案建置或執行遇到問題（逾時 >5 分鐘、錯誤無法解決），**直接跳過 Phase 1 剩餘任務和所有帶 ⚠️ 的測試任務**，改用手動測試驗證功能。

- [ ] T004 建立測試專案 BNICalculate.Tests（使用 `dotnet new xunit`）
- [ ] T005 新增測試專案參考到主專案（在 BNICalculate.Tests.csproj 中）
- [ ] T006 安裝 Microsoft.AspNetCore.Mvc.Testing 套件（指定版本 8.0.x）
- [ ] T007 驗證測試專案建置（執行 `dotnet build BNICalculate.Tests`，**逾時限制 2 分鐘**）
- [ ] T008 驗證測試專案執行（執行 `dotnet test BNICalculate.Tests --no-build`，**逾時限制 30 秒**）
- [ ] T009 建立測試專案目錄結構（Integration/Pages/）
- [ ] T010 建立 Usings.cs 定義全域 using 宣告

**預期時間**: 30 分鐘（如遇問題立即跳過）

**檢查點**: 測試專案設置完成，可執行 `dotnet test` 無錯誤
**失敗處理**: 如 T007 或 T008 失敗，記錄問題並標記「使用手動測試模式」

---

## Phase 2: Foundational (基礎必要條件)

**目的**: 設置專案配置，確保符合憲章要求

**⚠️ 關鍵**: 在實作任何使用者故事前必須完成此階段

- [ ] T011 在 BNICalculate.csproj 中啟用 `<Nullable>enable</Nullable>`
- [ ] T012 在 BNICalculate.csproj 中啟用 `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
- [ ] T013 驗證 .editorconfig 存在且配置正確
- [ ] T014 執行 `dotnet build` 確認無警告或錯誤

**預期時間**: 15 分鐘

**檢查點**: 專案建置成功，符合憲章的程式碼品質標準

---

## Phase 3: User Story 1 - 計算個人 BMI 值 (Priority: P1) 🎯 MVP

**目標**: 實作核心 BMI 計算功能，使用者可輸入身高體重並看到計算結果和分類

**獨立測試**: 使用者開啟 /BMI 頁面，輸入身高 1.75 和體重 70，點擊「計算」，看到「BMI: 22.9」和「分類: 正常」

### 測試 for User Story 1（TDD - 先寫測試）⚠️

> **備案模式**: 如果 Phase 1 測試環境設置失敗，**跳過 T015-T023**，直接進入「實作 for User Story 1」並使用手動測試。

> **注意：先撰寫這些測試，確保它們失敗後再實作**

- [ ] T015 [US1] 建立 BMIPageTests.cs 在 BNICalculate.Tests/Integration/Pages/
- [ ] T016 [US1] 撰寫測試：驗證 /BMI 頁面回應 HTTP 200
- [ ] T017 [US1] 撰寫測試：驗證頁面包含標題「BMI 計算器」
- [ ] T018 [US1] 撰寫測試：驗證頁面包含身高輸入欄位（id="height"）
- [ ] T019 [US1] 撰寫測試：驗證頁面包含體重輸入欄位（id="weight"）
- [ ] T020 [US1] 撰寫測試：驗證頁面包含「計算」按鈕（id="calculate-btn"）
- [ ] T021 [US1] 撰寫測試：驗證頁面包含結果顯示區域（id="bmi-value" 和 id="bmi-category"）
- [ ] T022 [US1] 撰寫測試：驗證頁面載入 bmi.js 腳本檔案
- [ ] T023 [US1] 執行測試確認全部失敗（Red）（**逾時限制 1 分鐘**）

**預期時間**: 1 小時（自動測試模式）/ 0 分鐘（手動測試模式）

**失敗處理**: 如 T023 逾時或卡住，強制終止並切換至手動測試模式

### 實作 for User Story 1

#### Razor Page 實作

- [ ] T024 [US1] 建立 Pages/BMI.cshtml.cs（PageModel 類別）
- [ ] T025 [US1] 在 BMI.cshtml.cs 新增 XML 文件註解（繁體中文）
- [ ] T026 [US1] 實作 OnGet() 方法（空實作，無後端邏輯）
- [ ] T027 [US1] 建立 Pages/BMI.cshtml（Razor 視圖）
- [ ] T028 [US1] 在 BMI.cshtml 新增頁面標題「BMI 計算器」（使用 h1）
- [ ] T029 [US1] 新增身高輸入欄位（label: 身高 (公尺), id: height, type: number, step: 0.01）
- [ ] T030 [US1] 新增體重輸入欄位（label: 體重 (公斤), id: weight, type: number, step: 0.1）
- [ ] T031 [US1] 新增「計算」按鈕（id: calculate-btn, type: button）
- [ ] T032 [US1] 新增結果顯示區域（div id="bmi-value" 和 div id="bmi-category"）
- [ ] T033 [US1] 引用 bmi.js 和 bmi.css（在 @section Scripts 和頁面頂端）

**預期時間**: 1 小時

#### JavaScript 實作

- [ ] T034 [US1] 建立 wwwroot/js/bmi.js
- [ ] T035 [US1] 實作 calculateBMI() 函式（讀取輸入值）
- [ ] T036 [US1] 實作 BMI 計算邏輯（BMI = weight / (height²)）
- [ ] T037 [US1] 實作四捨五入至小數點一位（Math.round(bmi * 10) / 10）
- [ ] T038 [US1] 實作 getBMICategory(bmi) 函式（6 個 WHO 分類區間）
- [ ] T039 [US1] 實作顯示結果邏輯（更新 #bmi-value 和 #bmi-category）
- [ ] T040 [US1] 綁定「計算」按鈕的 click 事件到 calculateBMI()
- [ ] T041 [US1] 測試極端值處理（身高 3 公尺、體重 500 公斤）

**預期時間**: 1.5 小時

#### 驗證與錯誤處理

- [ ] T042 [US1] 實作 validateInput() 函式在 bmi.js
- [ ] T043 [US1] 新增空值驗證（顯示「請輸入完整的身高和體重資料」）
- [ ] T044 [US1] 新增數字格式驗證（使用 isNaN()，顯示「請輸入數字」）
- [ ] T045 [US1] 新增正數驗證（檢查 > 0，顯示「請輸入有效的{欄位}值（大於 0）」）
- [ ] T046 [US1] 實作 displayError(field, message) 函式
- [ ] T047 [US1] 在輸入欄位下方動態插入紅色錯誤訊息（span.error-message）
- [ ] T048 [US1] 實作 clearErrors() 函式移除所有錯誤訊息
- [ ] T049 [US1] 在計算前呼叫 validateInput() 並處理錯誤

**預期時間**: 1 小時

#### CSS 樣式

- [ ] T050 [US1] 建立 wwwroot/css/bmi.css
- [ ] T051 [US1] 實作容器樣式（置中，max-width: 500px）
- [ ] T052 [US1] 實作表單樣式（輸入欄位間距、邊框、padding）
- [ ] T053 [US1] 實作按鈕樣式（簡潔、無圓角特效、並排顯示）
- [ ] T054 [US1] 實作錯誤訊息樣式（紅色文字，font-size: 0.9em）
- [ ] T055 [US1] 實作結果顯示區域樣式（綠色邊框、padding、margin）
- [ ] T056 [US1] 確保 CSS 檔案大小 <5KB

**預期時間**: 45 分鐘

#### 測試驗證（自動測試模式）⚠️

> **備案模式**: 如使用手動測試模式，跳過 T057-T058，直接執行 T059-T064

- [ ] T057 [US1] 執行整合測試套件（`dotnet test`）（**逾時限制 2 分鐘**）
- [ ] T058 [US1] 驗證所有測試通過（Green）

**失敗處理**: 如測試逾時或卡住，強制終止並切換至手動測試

#### 測試驗證（手動測試模式）

- [ ] T059 [US1] 啟動應用程式（`dotnet run`）並手動測試
- [ ] T060 [US1] 測試案例 1：身高 1.75、體重 70 → BMI 22.9、正常
- [ ] T061 [US1] 測試案例 2：身高 1.60、體重 45 → BMI 17.6、過輕
- [ ] T062 [US1] 測試案例 3：身高 1.70、體重 90 → BMI 31.1、中度肥胖
- [ ] T063 [US1] 測試案例 4：修改輸入值並重新計算
- [ ] T064 [US1] 測試邊界情況：空值、負數、非數字、極端值

**預期時間**: 45 分鐘（自動測試）/ 30 分鐘（手動測試）

**檢查點**: User Story 1 完全功能正常，可獨立測試，符合所有驗收標準

---

## Phase 4: User Story 2 - 清除輸入資料 (Priority: P2)

**目標**: 新增清除按鈕，使用者可快速重置所有欄位和結果

**獨立測試**: 使用者輸入資料並計算後，點擊「清除」按鈕，所有欄位和結果被清空

**依賴**: User Story 1 必須完成（需要輸入欄位和結果區域）

### 測試 for User Story 2（TDD - 先寫測試）⚠️

> **備案模式**: 如使用手動測試模式，跳過 T065-T066，直接進入「實作 for User Story 2」

- [ ] T065 [US2] 在 BMIPageTests.cs 新增測試：驗證頁面包含「清除」按鈕（id="clear-btn"）
- [ ] T066 [US2] 執行測試確認失敗（Red）（**逾時限制 30 秒**）

**預期時間**: 10 分鐘（自動測試模式）/ 0 分鐘（手動測試模式）

### 實作 for User Story 2

- [ ] T067 [US2] 在 BMI.cshtml 新增「清除」按鈕（id: clear-btn, type: button）
- [ ] T068 [US2] 確保「計算」和「清除」按鈕並排顯示（使用 CSS flex 或 inline-block）
- [ ] T069 [US2] 在 bmi.js 實作 clearForm() 函式
- [ ] T070 [US2] clearForm() 清空身高和體重輸入欄位（設為空字串）
- [ ] T071 [US2] clearForm() 清空結果顯示區域（#bmi-value 和 #bmi-category）
- [ ] T072 [US2] clearForm() 呼叫 clearErrors() 移除錯誤訊息
- [ ] T073 [US2] 綁定「清除」按鈕的 click 事件到 clearForm()
- [ ] T074 [US2] 在 bmi.css 調整按鈕樣式確保並排顯示

**預期時間**: 30 分鐘

### 測試驗證（自動測試模式）⚠️

> **備案模式**: 如使用手動測試模式，跳過 T075-T076，直接執行 T077-T079

- [ ] T075 [US2] 執行整合測試確認通過（Green）（**逾時限制 1 分鐘**）
- [ ] T076 [US2] 驗證測試結果

### 測試驗證（手動測試模式）

- [ ] T077 [US2] 手動測試：輸入資料並計算後點擊清除
- [ ] T078 [US2] 驗證所有輸入欄位清空和結果區域清空
- [ ] T079 [US2] 測試案例：僅輸入部分資料（未計算）後點擊清除

**預期時間**: 15 分鐘

**檢查點**: User Story 2 完全功能正常，可獨立測試

---

## Phase 5: Polish & Cross-Cutting Concerns (最終優化)

**目的**: 程式碼品質、效能驗證、文件完善

### 程式碼品質

- [ ] T080 執行 `dotnet format` 確保程式碼格式一致
- [ ] T081 執行 `dotnet build` 確認零警告
- [ ] T082 檢查所有 public 方法有 XML 文件註解（繁體中文）
- [ ] T083 審查 JavaScript 程式碼命名（camelCase）
- [ ] T084 審查 C# 程式碼命名（PascalCase for public, camelCase for private）

**預期時間**: 30 分鐘

### 效能驗證

- [ ] T085 測試頁面載入時間（使用 Chrome DevTools，目標 <2 秒）
- [ ] T086 測試 BMI 計算回應時間（使用 console.time，目標 <1 秒）
- [ ] T087 驗證 CSS 檔案大小（應 <5KB）
- [ ] T088 驗證 JavaScript 檔案大小（應 <10KB）
- [ ] T089 測試 TTI (Time to Interactive，目標 <3 秒)

**預期時間**: 30 分鐘

### 跨瀏覽器測試

- [ ] T090 在 Chrome 測試所有功能
- [ ] T091 在 Firefox 測試所有功能
- [ ] T092 在 Safari 測試所有功能（如有 macOS）
- [ ] T093 在 Edge 測試所有功能

**預期時間**: 30 分鐘

### 響應式設計驗證

- [ ] T094 測試手機尺寸（375px 寬度）
- [ ] T095 測試平板尺寸（768px 寬度）
- [ ] T096 測試桌面尺寸（1920px 寬度）

**預期時間**: 20 分鐘

### 無障礙檢查

- [ ] T097 驗證所有輸入欄位有 label
- [ ] T098 驗證可使用 Tab 鍵導覽
- [ ] T099 驗證錯誤訊息有適當的 ARIA 屬性（如需要）

**預期時間**: 20 分鐘

### 文件更新

- [ ] T100 更新 README.md 包含 BMI 功能說明
- [ ] T101 在 quickstart.md 新增實際測試結果
- [ ] T102 截圖並新增到文件（如需要）

**預期時間**: 20 分鐘

**檢查點**: 所有品質標準符合，準備提交 PR

---

## 測試環境故障排除與備案方案 🔧

### 快速決策流程

```text
Phase 1 環境驗證 (T001-T003)
  ↓
是否通過？
  ├─ ✅ 是 → 繼續測試專案建置 (T004-T010)
  └─ ❌ 否 → 記錄問題，使用手動測試模式
        ↓
測試專案建置 (T007-T008)
  ↓
是否在 2 分鐘內完成？
  ├─ ✅ 是 → 使用自動測試模式（TDD）
  └─ ❌ 否 → 強制終止，切換手動測試模式
```

### 常見問題與解決方案

#### 問題 1: 測試專案建置逾時或卡住

**症狀**: `dotnet build BNICalculate.Tests` 超過 2 分鐘無回應

**解決方案**:
1. 按 `Ctrl+C` 強制終止
2. 檢查 NuGet 套件來源（執行 `dotnet nuget list source`）
3. 如問題持續，**跳過所有測試任務**，使用手動測試

**備案**: 刪除測試專案，僅保留主專案

#### 問題 2: WebApplicationFactory 啟動失敗

**症狀**: 測試執行時拋出「無法啟動應用程式」錯誤

**解決方案**:
1. 檢查 `Program.cs` 是否有 `public partial class Program { }` 宣告
2. 檢查測試專案是否正確引用主專案
3. 如問題持續，**切換至手動測試模式**

**備案**: 使用 `dotnet run` 啟動應用程式，手動在瀏覽器測試

#### 問題 3: 測試執行逾時

**症狀**: `dotnet test` 執行超過 2 分鐘

**解決方案**:
1. 按 `Ctrl+C` 強制終止
2. 執行 `dotnet clean` 清理專案
3. 重新建置並測試
4. 如問題持續，**切換至手動測試模式**

**備案**: 直接使用瀏覽器手動驗證功能

#### 問題 4: 端口衝突

**症狀**: 應用程式或測試無法啟動，提示端口 5000 已被佔用

**解決方案**:
1. macOS/Linux: `lsof -ti:5000 | xargs kill -9`
2. Windows: `netstat -ano | findstr :5000` 然後終止進程
3. 或修改 `launchSettings.json` 使用其他端口

### 手動測試模式操作指引

如果選擇**手動測試模式**，請遵循以下步驟：

#### 跳過的任務

所有標記 ⚠️ 的測試任務：
- Phase 3: T015-T023（User Story 1 測試撰寫）
- Phase 3: T057-T058（自動測試驗證）
- Phase 4: T065-T066（User Story 2 測試撰寫）
- Phase 4: T075-T076（自動測試驗證）

#### 手動測試檢查清單

**User Story 1 驗證**:
1. 執行 `dotnet run --project BNICalculate`
2. 開啟瀏覽器訪問 `http://localhost:5000/BMI`
3. 測試案例：
   - [ ] 輸入身高 1.75、體重 70 → 顯示 BMI 22.9、正常
   - [ ] 輸入身高 1.60、體重 45 → 顯示 BMI 17.6、過輕
   - [ ] 輸入身高 1.70、體重 90 → 顯示 BMI 31.1、中度肥胖
   - [ ] 空值測試 → 顯示錯誤訊息
   - [ ] 負數測試 → 顯示錯誤訊息
   - [ ] 非數字測試 → 顯示錯誤訊息

**User Story 2 驗證**:
1. 繼續在瀏覽器中操作
2. 測試案例：
   - [ ] 輸入資料並計算
   - [ ] 點擊「清除」按鈕
   - [ ] 確認所有欄位和結果被清空

### 測試模式比較

| 項目 | 自動測試模式 (TDD) | 手動測試模式 |
|------|-------------------|-------------|
| **設置時間** | 40 分鐘（含環境驗證） | 10 分鐘（僅環境驗證） |
| **測試撰寫** | 需要（1 小時） | 不需要 |
| **測試執行** | 自動（2-3 分鐘） | 手動（15-30 分鐘） |
| **回歸測試** | 快速重複執行 | 每次手動驗證 |
| **適用場景** | 長期維護專案 | 快速原型/練習專案 |
| **總時間（MVP）** | 7 小時 | 6.5 小時 |

### 建議

**選擇自動測試模式如果**:
- ✅ 環境驗證通過（T001-T003）
- ✅ 測試專案建置在 2 分鐘內完成
- ✅ 計畫長期維護此專案

**選擇手動測試模式如果**:
- ⚠️ 環境驗證失敗或測試建置逾時
- ⚠️ 這是短期練習專案
- ⚠️ 希望快速看到功能效果

---

## Implementation Strategy (實作策略)

### MVP 優先（推薦）

**第一階段 - MVP**（User Story 1）:

1. 完成 Phase 1 (Setup)
2. 完成 Phase 2 (Foundational)
3. 完成 Phase 3 (User Story 1)
4. **部署 MVP** - 使用者已可使用核心功能

**第二階段 - 增量交付**（User Story 2）:

1. 完成 Phase 4 (User Story 2)
2. 部署更新

**第三階段 - 優化**:

1. 完成 Phase 5 (Polish)
2. 最終部署

### 全功能一次交付

如果偏好一次完成所有功能：

1. Phase 1 → Phase 2 → Phase 3 → Phase 4 → Phase 5
2. 單次部署

---

## Dependencies & Execution Order (相依性與執行順序)

### 必須循序執行

```text
Phase 1 (Setup)
  ↓
Phase 2 (Foundational)
  ↓
Phase 3 (User Story 1) - MVP
  ↓
Phase 4 (User Story 2) - 依賴 User Story 1
  ↓
Phase 5 (Polish)
```

### User Story 獨立性

- ✅ **User Story 1**: 完全獨立，可單獨部署
- ⚠️ **User Story 2**: 依賴 User Story 1（需要輸入欄位和結果區域）

### 階段內執行順序

#### Phase 1: Setup

所有任務可獨立執行（T003, T004, T005）

#### Phase 2: Foundational

所有任務可獨立執行（T006, T007, T008）

#### Phase 3: User Story 1

執行區塊 1（測試撰寫）:

- T010-T017 依序執行

執行區塊 2（實作）:

- T019-T028 依序執行（PageModel 和 Razor View）
- T029-T036 依序執行（bmi.js）
- T045-T051 依序執行（bmi.css）

執行區塊 3（驗證）:

- T037-T044 依序執行（驗證邏輯）

#### Phase 5: Polish

所有任務按類別順序執行

---

## Task Summary (任務摘要)

### 統計

- **總任務數**: 102 個（原 97 個 + 5 個環境驗證任務）
- **Phase 1 (Setup)**: 10 個任務（40 分鐘，含環境驗證）
- **Phase 2 (Foundational)**: 4 個任務（15 分鐘）
- **Phase 3 (User Story 1 - MVP)**: 50 個任務（6 小時）
  - 測試: 9 個任務（1 小時，自動模式）/ 0 個（手動模式）
  - Razor Page: 10 個任務（1 小時）
  - JavaScript: 8 個任務（1.5 小時）
  - 驗證: 8 個任務（1 小時）
  - CSS: 7 個任務（45 分鐘）
  - 測試驗證: 8 個任務（45 分鐘自動 / 30 分鐘手動）
- **Phase 4 (User Story 2)**: 15 個任務（55 分鐘）
- **Phase 5 (Polish)**: 23 個任務（2.5 小時）

**總預估時間**: 約 9.5-10 小時（依測試模式而定）

### MVP 範圍

建議 MVP 包含：

- ✅ Phase 1: Setup（含環境驗證）
- ✅ Phase 2: Foundational
- ✅ Phase 3: User Story 1（核心 BMI 計算功能）

**MVP 預估時間**: 7 小時（自動測試模式）/ 6.5 小時（手動測試模式）

---

## Independent Test Criteria (獨立測試標準)

### User Story 1

**驗證清單**:

- [ ] 瀏覽 http://localhost:5000/BMI 回應 HTTP 200
- [ ] 頁面顯示標題「BMI 計算器」
- [ ] 可輸入身高和體重
- [ ] 點擊「計算」顯示 BMI 值和分類
- [ ] 輸入身高 1.75、體重 70 → 顯示 BMI 22.9、正常
- [ ] 輸入空值 → 顯示錯誤訊息
- [ ] 輸入負數 → 顯示錯誤訊息

**工具**: 手動測試 + xUnit 整合測試

### User Story 2

**驗證清單**:

- [ ] User Story 1 的所有測試通過
- [ ] 頁面顯示「清除」按鈕
- [ ] 點擊「清除」後所有欄位清空
- [ ] 點擊「清除」後結果區域清空
- [ ] 點擊「清除」後錯誤訊息消失

**工具**: 手動測試 + xUnit 整合測試

---

## Next Steps (下一步)

1. ✅ 審查此任務清單
2. ✅ 開始 Phase 1: Setup
3. ✅ 遵循 TDD 工作流程（Red → Green → Refactor）
4. ✅ 完成每個任務後勾選核取方塊
5. ✅ 在每個階段檢查點驗證功能
6. ✅ 完成 MVP 後考慮部署
7. ✅ 持續進行增量交付

**準備好開始了嗎？從 T001 開始吧！** 🚀

---

## Notes (備註)

- **TDD 工作流程**: 所有實作任務前先撰寫測試
- **程式碼審查**: 每個 Phase 完成後建議進行程式碼審查
- **提交頻率**: 建議每完成一個小節（例如：Razor Page 實作）就提交一次
- **測試優先**: 確保測試失敗後再實作，避免假陽性
- **憲章合規**: 所有任務必須符合專案憲章的程式碼品質標準
