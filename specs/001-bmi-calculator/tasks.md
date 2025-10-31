# Tasks: BMI 計算器網頁

**Feature**: 001-bmi-calculator  
**Branch**: `001-bmi-calculator`  
**Input**: 設計文件來自 `/specs/001-bmi-calculator/`  
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, quickstart.md ✅

**Tests**: 此專案採用 TDD 方法，包含整合測試任務。

**Organization**: 任務按使用者故事分組，使每個故事可獨立實作和測試。

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 可並行執行（不同檔案，無相依性）
- **[Story]**: 任務所屬使用者故事（例如：US1, US2）
- 描述包含確切檔案路徑

## Path Conventions

本專案使用 ASP.NET Core Razor Pages 標準結構：

- **主專案**: `BNICalculate/`
- **測試專案**: `BNICalculate.Tests/`
- **文件**: `specs/001-bmi-calculator/`

---

## Phase 1: Setup (共用基礎設施)

**目的**: 建立測試專案和基礎設定

- [ ] T001 建立測試專案 BNICalculate.Tests（使用 `dotnet new xunit`）
- [ ] T002 新增測試專案參考到主專案（在 BNICalculate.Tests.csproj 中）
- [ ] T003 [P] 安裝 Microsoft.AspNetCore.Mvc.Testing 套件到測試專案
- [ ] T004 [P] 建立測試專案目錄結構（Integration/Pages/）
- [ ] T005 [P] 建立 Usings.cs 定義全域 using 宣告

**預期時間**: 30 分鐘

**檢查點**: 測試專案設置完成，可執行 `dotnet test` 無錯誤

---

## Phase 2: Foundational (基礎必要條件)

**目的**: 設置專案配置，確保符合憲章要求

**⚠️ 關鍵**: 在實作任何使用者故事前必須完成此階段

- [ ] T006 在 BNICalculate.csproj 中啟用 `<Nullable>enable</Nullable>`
- [ ] T007 在 BNICalculate.csproj 中啟用 `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
- [ ] T008 驗證 .editorconfig 存在且配置正確
- [ ] T009 執行 `dotnet build` 確認無警告或錯誤

**預期時間**: 15 分鐘

**檢查點**: 專案建置成功，符合憲章的程式碼品質標準

---

## Phase 3: User Story 1 - 計算個人 BMI 值 (Priority: P1) 🎯 MVP

**目標**: 實作核心 BMI 計算功能，使用者可輸入身高體重並看到計算結果和分類

**獨立測試**: 使用者開啟 /BMI 頁面，輸入身高 1.75 和體重 70，點擊「計算」，看到「BMI: 22.9」和「分類: 正常」

### 測試 for User Story 1（TDD - 先寫測試）⚠️

> **注意：先撰寫這些測試，確保它們失敗後再實作**

- [ ] T010 [P] [US1] 建立 BMIPageTests.cs 在 BNICalculate.Tests/Integration/Pages/
- [ ] T011 [US1] 撰寫測試：驗證 /BMI 頁面回應 HTTP 200
- [ ] T012 [US1] 撰寫測試：驗證頁面包含標題「BMI 計算器」
- [ ] T013 [US1] 撰寫測試：驗證頁面包含身高輸入欄位（id="height"）
- [ ] T014 [US1] 撰寫測試：驗證頁面包含體重輸入欄位（id="weight"）
- [ ] T015 [US1] 撰寫測試：驗證頁面包含「計算」按鈕（id="calculate-btn"）
- [ ] T016 [US1] 撰寫測試：驗證頁面包含結果顯示區域（id="bmi-value" 和 id="bmi-category"）
- [ ] T017 [US1] 撰寫測試：驗證頁面載入 bmi.js 腳本檔案
- [ ] T018 [US1] 執行測試確認全部失敗（Red）

**預期時間**: 1 小時

### 實作 for User Story 1

#### Razor Page 實作

- [ ] T019 [P] [US1] 建立 Pages/BMI.cshtml.cs（PageModel 類別）
- [ ] T020 [US1] 在 BMI.cshtml.cs 新增 XML 文件註解（繁體中文）
- [ ] T021 [US1] 實作 OnGet() 方法（空實作，無後端邏輯）
- [ ] T022 [P] [US1] 建立 Pages/BMI.cshtml（Razor 視圖）
- [ ] T023 [US1] 在 BMI.cshtml 新增頁面標題「BMI 計算器」（使用 h1）
- [ ] T024 [US1] 新增身高輸入欄位（label: 身高 (公尺), id: height, type: number, step: 0.01）
- [ ] T025 [US1] 新增體重輸入欄位（label: 體重 (公斤), id: weight, type: number, step: 0.1）
- [ ] T026 [US1] 新增「計算」按鈕（id: calculate-btn, type: button）
- [ ] T027 [US1] 新增結果顯示區域（div id="bmi-value" 和 div id="bmi-category"）
- [ ] T028 [US1] 引用 bmi.js 和 bmi.css（在 @section Scripts 和頁面頂端）

**預期時間**: 1 小時

#### JavaScript 實作

- [ ] T029 [P] [US1] 建立 wwwroot/js/bmi.js
- [ ] T030 [US1] 實作 calculateBMI() 函式（讀取輸入值）
- [ ] T031 [US1] 實作 BMI 計算邏輯（BMI = weight / (height²)）
- [ ] T032 [US1] 實作四捨五入至小數點一位（Math.round(bmi * 10) / 10）
- [ ] T033 [US1] 實作 getBMICategory(bmi) 函式（6 個 WHO 分類區間）
- [ ] T034 [US1] 實作顯示結果邏輯（更新 #bmi-value 和 #bmi-category）
- [ ] T035 [US1] 綁定「計算」按鈕的 click 事件到 calculateBMI()
- [ ] T036 [US1] 測試極端值處理（身高 3 公尺、體重 500 公斤）

**預期時間**: 1.5 小時

#### 驗證與錯誤處理

- [ ] T037 [P] [US1] 實作 validateInput() 函式在 bmi.js
- [ ] T038 [US1] 新增空值驗證（顯示「請輸入完整的身高和體重資料」）
- [ ] T039 [US1] 新增數字格式驗證（使用 isNaN()，顯示「請輸入數字」）
- [ ] T040 [US1] 新增正數驗證（檢查 > 0，顯示「請輸入有效的{欄位}值（大於 0）」）
- [ ] T041 [US1] 實作 displayError(field, message) 函式
- [ ] T042 [US1] 在輸入欄位下方動態插入紅色錯誤訊息（span.error-message）
- [ ] T043 [US1] 實作 clearErrors() 函式移除所有錯誤訊息
- [ ] T044 [US1] 在計算前呼叫 validateInput() 並處理錯誤

**預期時間**: 1 小時

#### CSS 樣式

- [ ] T045 [P] [US1] 建立 wwwroot/css/bmi.css
- [ ] T046 [US1] 實作容器樣式（置中，max-width: 500px）
- [ ] T047 [US1] 實作表單樣式（輸入欄位間距、邊框、padding）
- [ ] T048 [US1] 實作按鈕樣式（簡潔、無圓角特效、並排顯示）
- [ ] T049 [US1] 實作錯誤訊息樣式（紅色文字，font-size: 0.9em）
- [ ] T050 [US1] 實作結果顯示區域樣式（綠色邊框、padding、margin）
- [ ] T051 [US1] 確保 CSS 檔案大小 <5KB

**預期時間**: 45 分鐘

#### 測試驗證

- [ ] T052 [US1] 執行整合測試套件（`dotnet test`）
- [ ] T053 [US1] 驗證所有測試通過（Green）
- [ ] T054 [US1] 啟動應用程式（`dotnet run`）並手動測試
- [ ] T055 [US1] 測試案例 1：身高 1.75、體重 70 → BMI 22.9、正常
- [ ] T056 [US1] 測試案例 2：身高 1.60、體重 45 → BMI 17.6、過輕
- [ ] T057 [US1] 測試案例 3：身高 1.70、體重 90 → BMI 31.1、中度肥胖
- [ ] T058 [US1] 測試案例 4：修改輸入值並重新計算
- [ ] T059 [US1] 測試邊界情況：空值、負數、非數字、極端值

**預期時間**: 45 分鐘

**檢查點**: User Story 1 完全功能正常，可獨立測試，符合所有驗收標準

---

## Phase 4: User Story 2 - 清除輸入資料 (Priority: P2)

**目標**: 新增清除按鈕，使用者可快速重置所有欄位和結果

**獨立測試**: 使用者輸入資料並計算後，點擊「清除」按鈕，所有欄位和結果被清空

**依賴**: User Story 1 必須完成（需要輸入欄位和結果區域）

### 測試 for User Story 2（TDD - 先寫測試）⚠️

- [ ] T060 [P] [US2] 在 BMIPageTests.cs 新增測試：驗證頁面包含「清除」按鈕（id="clear-btn"）
- [ ] T061 [US2] 執行測試確認失敗（Red）

**預期時間**: 10 分鐘

### 實作 for User Story 2

- [ ] T062 [US2] 在 BMI.cshtml 新增「清除」按鈕（id: clear-btn, type: button）
- [ ] T063 [US2] 確保「計算」和「清除」按鈕並排顯示（使用 CSS flex 或 inline-block）
- [ ] T064 [US2] 在 bmi.js 實作 clearForm() 函式
- [ ] T065 [US2] clearForm() 清空身高和體重輸入欄位（設為空字串）
- [ ] T066 [US2] clearForm() 清空結果顯示區域（#bmi-value 和 #bmi-category）
- [ ] T067 [US2] clearForm() 呼叫 clearErrors() 移除錯誤訊息
- [ ] T068 [US2] 綁定「清除」按鈕的 click 事件到 clearForm()
- [ ] T069 [US2] 在 bmi.css 調整按鈕樣式確保並排顯示

**預期時間**: 30 分鐘

### 測試驗證

- [ ] T070 [US2] 執行整合測試確認通過（Green）
- [ ] T071 [US2] 手動測試：輸入資料並計算後點擊清除
- [ ] T072 [US2] 驗證所有輸入欄位清空
- [ ] T073 [US2] 驗證結果顯示區域清空
- [ ] T074 [US2] 測試案例：僅輸入部分資料（未計算）後點擊清除

**預期時間**: 15 分鐘

**檢查點**: User Story 2 完全功能正常，可獨立測試

---

## Phase 5: Polish & Cross-Cutting Concerns (最終優化)

**目的**: 程式碼品質、效能驗證、文件完善

### 程式碼品質

- [ ] T075 [P] 執行 `dotnet format` 確保程式碼格式一致
- [ ] T076 [P] 執行 `dotnet build` 確認零警告
- [ ] T077 [P] 檢查所有 public 方法有 XML 文件註解（繁體中文）
- [ ] T078 [P] 審查 JavaScript 程式碼命名（camelCase）
- [ ] T079 [P] 審查 C# 程式碼命名（PascalCase for public, camelCase for private）

**預期時間**: 30 分鐘

### 效能驗證

- [ ] T080 [P] 測試頁面載入時間（使用 Chrome DevTools，目標 <2 秒）
- [ ] T081 [P] 測試 BMI 計算回應時間（使用 console.time，目標 <1 秒）
- [ ] T082 [P] 驗證 CSS 檔案大小（應 <5KB）
- [ ] T083 [P] 驗證 JavaScript 檔案大小（應 <10KB）
- [ ] T084 [P] 測試 TTI (Time to Interactive，目標 <3 秒)

**預期時間**: 30 分鐘

### 跨瀏覽器測試

- [ ] T085 [P] 在 Chrome 測試所有功能
- [ ] T086 [P] 在 Firefox 測試所有功能
- [ ] T087 [P] 在 Safari 測試所有功能（如有 macOS）
- [ ] T088 [P] 在 Edge 測試所有功能

**預期時間**: 30 分鐘

### 響應式設計驗證

- [ ] T089 [P] 測試手機尺寸（375px 寬度）
- [ ] T090 [P] 測試平板尺寸（768px 寬度）
- [ ] T091 [P] 測試桌面尺寸（1920px 寬度）

**預期時間**: 20 分鐘

### 無障礙檢查

- [ ] T092 [P] 驗證所有輸入欄位有 label
- [ ] T093 [P] 驗證可使用 Tab 鍵導覽
- [ ] T094 [P] 驗證錯誤訊息有適當的 ARIA 屬性（如需要）

**預期時間**: 20 分鐘

### 文件更新

- [ ] T095 [P] 更新 README.md 包含 BMI 功能說明
- [ ] T096 [P] 在 quickstart.md 新增實際測試結果
- [ ] T097 [P] 截圖並新增到文件（如需要）

**預期時間**: 20 分鐘

**檢查點**: 所有品質標準符合，準備提交 PR

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

### 階段內並行機會

#### Phase 1: Setup

- T003, T004, T005 可並行（[P] 標記）

#### Phase 2: Foundational

- T006, T007, T008 可並行（不同設定檔）

#### Phase 3: User Story 1

並行區塊 1（測試撰寫）:

- T010-T017 可並行（獨立測試案例）

並行區塊 2（實作）:

- T019 (PageModel) 和 T022 (Razor View) 可並行
- T029 (bmi.js) 和 T045 (bmi.css) 可並行

並行區塊 3（驗證）:

- T037 (驗證邏輯) 獨立於 CSS 實作

#### Phase 5: Polish

- 所有任務標記 [P] 可並行執行

---

## Task Summary (任務摘要)

### 統計

- **總任務數**: 97 個
- **Phase 1 (Setup)**: 5 個任務（30 分鐘）
- **Phase 2 (Foundational)**: 4 個任務（15 分鐘）
- **Phase 3 (User Story 1 - MVP)**: 50 個任務（6 小時）
  - 測試: 9 個任務（1 小時）
  - Razor Page: 10 個任務（1 小時）
  - JavaScript: 8 個任務（1.5 小時）
  - 驗證: 8 個任務（1 小時）
  - CSS: 7 個任務（45 分鐘）
  - 測試驗證: 8 個任務（45 分鐘）
- **Phase 4 (User Story 2)**: 15 個任務（55 分鐘）
- **Phase 5 (Polish)**: 23 個任務（2.5 小時）

**總預估時間**: 約 9.5 小時

### MVP 範圍

建議 MVP 包含：

- ✅ Phase 1: Setup
- ✅ Phase 2: Foundational
- ✅ Phase 3: User Story 1（核心 BMI 計算功能）

**MVP 預估時間**: 6.75 小時

### 並行機會

- **Setup 階段**: 3 個任務可並行
- **User Story 1**: 多個任務可並行（測試撰寫、前後端分離）
- **Polish 階段**: 23 個任務大多可並行

**潛在節省時間**: 如 2 人並行開發，可節省 2-3 小時

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
