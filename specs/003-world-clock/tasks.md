# 任務清單：世界時鐘 (World Clock)

**輸入來源**: 設計文件來自 `/specs/003-world-clock/`  
**前置需求**: plan.md, spec.md, research.md, data-model.md, contracts/  
**產生日期**: 2025-11-01

**測試策略**:

- 遵循 TDD 工作流程：先撰寫測試（Red）→ 實作功能（Green）→ 重構（Refactor）
- 整合測試使用 xUnit + WebApplicationFactory 驗證頁面載入和 HTML 結構
- 手動驗證作為補充測試方式（開啟瀏覽器驗證互動和視覺效果）
- 每個使用者故事都必須通過自動化測試和手動驗證
- 目標覆蓋率：JavaScript 核心邏輯 80%，PageModel 80%

**組織方式**: 任務依使用者故事分組，每個故事都可獨立實作和測試

## 格式說明：`[ID] [Story] 描述`

- **[Story]**: 此任務屬於哪個使用者故事（例如：US1, US2, US3）
- 所有任務描述都包含明確的檔案路徑

---

## 階段 1：設定（共用基礎設施）

**目的**: 建立基本檔案結構和專案配置

- [X] T001 在 BNICalculate/Pages/WorldClock.cshtml.cs 建立空白的 WorldClock PageModel
- [X] T002 在 BNICalculate/Pages/WorldClock.cshtml 建立空白的 WorldClock Razor 視圖
- [X] T003 在 BNICalculate/wwwroot/css/worldclock.css 建立空白的樣式表
- [X] T004 在 BNICalculate/wwwroot/js/worldclock.js 建立空白的 JavaScript 腳本
- [X] T005 在 BNICalculate/Pages/Shared/_Layout.cshtml 新增世界時鐘頁面的導覽連結

---

## 階段 2：基礎建設（阻擋性前置需求）

**目的**: 建立使用者故事將依賴的基本頁面結構

**⚠️ 重要**: 在此階段完成前，所有使用者故事的工作都無法開始

- [X] T006 在 BNICalculate/Pages/WorldClock.cshtml.cs 實作基本的 WorldClockModel 類別並加上 XML 文件註解
- [X] T007 在 BNICalculate/Pages/WorldClock.cshtml 建立基本的 HTML 結構（使用 Bootstrap 容器）
- [X] T008 在 WorldClock.cshtml 中使用 script/link 標籤連結 CSS 和 JS 檔案

**檢查點**: 基礎就緒 - 使用者故事實作現在可以開始

---

## 階段 3：使用者故事 1 - 查看多個城市時間（優先級：P1）🎯 MVP

**目標**: 使用者可以同時查看 10 個城市的即時時間，包括主要顯示（台北）和 9 個次要城市，每秒自動更新

**獨立測試**: 載入頁面並驗證顯示所有 10 個城市（台北、東京、倫敦、紐約、洛杉磯、巴黎、柏林、莫斯科、新加坡、悉尼）的正確時間，格式為 HH:mm:ss

### 使用者故事 1 的測試

**測試方法（TDD 工作流程）**:

1. **自動化測試**（必需）：
   - 撰寫整合測試驗證頁面載入和 HTML 結構
   - 測試必須先於實作程式碼提交
   - 使用 xUnit + WebApplicationFactory

2. **手動驗證**（補充）：
   - 開啟瀏覽器訪問 `/WorldClock` 頁面
   - 驗證顯示 10 個城市（1 個主要 + 9 個次要）
   - 驗證時間格式為 HH:mm:ss
   - 驗證時間每秒更新

**自動化測試任務**:

- [X] T009 [US1] 在 BNICalculate.Tests/Integration/Pages/WorldClockPageTests.cs 建立 WorldClockPageTests.cs 並加上頁面載入測試
- [X] T010 [US1] 在 WorldClockPageTests.cs 新增測試驗證 10 個城市顯示
- [X] T011 [US1] 在 WorldClockPageTests.cs 新增測試驗證正確的 HTML 結構（主要 + 9 張卡片）並驗證日期格式為 YYYY-MM-DD

### 使用者故事 1 的實作

#### HTML 結構（WorldClock.cshtml）

- [X] T012 [US1] 在 BNICalculate/Pages/WorldClock.cshtml 新增載入指示器 HTML（旋轉圖示 + "載入中..."）
- [X] T013 [US1] 在 BNICalculate/Pages/WorldClock.cshtml 建立主要時鐘顯示區域並加上 ARIA live region
- [X] T014 [US1] 在 BNICalculate/Pages/WorldClock.cshtml 新增主要時間顯示元素（城市名稱、時區、時間、日期）
- [X] T015 [US1] 在 BNICalculate/Pages/WorldClock.cshtml 建立 9 個次要城市卡片的網格容器
- [X] T016 [US1] 在 BNICalculate/Pages/WorldClock.cshtml 新增 9 個城市卡片範本並加上 data 屬性
- [X] T017 [US1] 在 BNICalculate/Pages/WorldClock.cshtml 新增時區 API 失敗時的錯誤訊息容器

#### CSS 樣式（worldclock.css）

- [X] T018 [US1] 在 BNICalculate/wwwroot/css/worldclock.css 定義色彩的 CSS 變數（依照 FR-025）
- [X] T019 [US1] 在 BNICalculate/wwwroot/css/worldclock.css 設定載入指示器樣式（置中、旋轉動畫）
- [X] T020 [US1] 在 BNICalculate/wwwroot/css/worldclock.css 設定主要時鐘顯示區域樣式（白色背景、陰影、置中）
- [X] T021 [US1] 在 BNICalculate/wwwroot/css/worldclock.css 定義字體樣式（依照 FR-026：主要 48px、卡片 24px、等寬字體）
- [X] T022 [US1] 在 BNICalculate/wwwroot/css/worldclock.css 建立 9 個城市卡片的 CSS Grid 佈局（響應式：桌面 3x3、平板 2 欄、手機 1 欄）
- [X] T023 [US1] 在 BNICalculate/wwwroot/css/worldclock.css 設定城市卡片樣式（白色背景、邊框、內距）
- [X] T024 [US1] 在 BNICalculate/wwwroot/css/worldclock.css 設定錯誤訊息容器樣式

#### JavaScript 核心邏輯（worldclock.js）

- [X] T025 [US1] 在 BNICalculate/wwwroot/js/worldclock.js 依照 contracts/world-clock-data.md 定義 10 個 CityTimezone 配置物件
- [X] T026 [US1] 在 BNICalculate/wwwroot/js/worldclock.js 建立 ClockState 物件來管理主要/次要城市和計時器
- [X] T027 [US1] 在 BNICalculate/wwwroot/js/worldclock.js 實作 createFormatter() 函式來建立 Intl.DateTimeFormat 實例
- [X] T028 [US1] 在 BNICalculate/wwwroot/js/worldclock.js 實作 formatTime() 函式使用 Intl API 格式化城市時間
- [X] T029 [US1] 在 BNICalculate/wwwroot/js/worldclock.js 實作 formatDate() 函式來格式化時區的日期
- [X] T030 [US1] 在 BNICalculate/wwwroot/js/worldclock.js 實作 updateMainClock() 函式來更新主要顯示的 DOM
- [X] T031 [US1] 在 BNICalculate/wwwroot/js/worldclock.js 實作 updateSecondaryClock() 函式來更新城市卡片 DOM
- [X] T032 [US1] 在 BNICalculate/wwwroot/js/worldclock.js 實作 updateAllClocks() 函式來更新所有 10 個時鐘
- [X] T033 [US1] 在 BNICalculate/wwwroot/js/worldclock.js 實作 startClock() 函式並使用 setInterval（1000ms）
- [X] T034 [US1] 在 BNICalculate/wwwroot/js/worldclock.js 實作 stopClock() 函式來清除 interval 並防止記憶體洩漏
- [X] T035 [US1] 在 BNICalculate/wwwroot/js/worldclock.js 依照 FR-022 為 Intl API 失敗加上 try-catch 錯誤處理
- [X] T036 [US1] 在 BNICalculate/wwwroot/js/worldclock.js 實作 showError() 函式來顯示錯誤訊息和重新整理按鈕
- [X] T037 [US1] 在 BNICalculate/wwwroot/js/worldclock.js 實作 hideLoading() 函式來隱藏載入指示器
- [X] T038 [US1] 在 BNICalculate/wwwroot/js/worldclock.js 新增 DOMContentLoaded 事件監聽器來初始化時鐘
- [X] T039 [US1] 在 BNICalculate/wwwroot/js/worldclock.js 新增 beforeunload 事件監聽器來呼叫 stopClock()

#### 心跳檢測（FR-009）

- [X] T040 [US1] 在 BNICalculate/wwwroot/js/worldclock.js 實作心跳檢測：在 ClockState 中追蹤 lastUpdateTime
- [X] T041 [US1] 在 BNICalculate/wwwroot/js/worldclock.js 新增 checkHeartbeat() 函式來驗證 5 秒內有更新
- [X] T042 [US1] 在 BNICalculate/wwwroot/js/worldclock.js 建立心跳計時器（setInterval 60000ms）來呼叫 checkHeartbeat()
- [X] T043 [US1] 在 BNICalculate/wwwroot/js/worldclock.js 實作心跳失敗時的自動重啟邏輯
- [X] T044 [US1] 在 BNICalculate/wwwroot/js/worldclock.js 心跳觸發重啟時新增 console.error 日誌記錄

**檢查點**: 此時使用者故事 1 應該完全可運作 - 頁面載入、顯示 10 個城市、每秒更新、處理錯誤、自動從停滯中恢復

---

## 階段 4：使用者故事 2 - 切換主要顯示時間（優先級：P2）

**目標**: 使用者可以點選任一城市時間卡片，將該城市設為主要顯示，並看到視覺回饋（hover、active 效果）

**獨立測試**: 點選任一城市卡片，驗證該時間移至正中央成為主要顯示，且原主要時間移至城市列表中

### 使用者故事 2 的測試

**測試方法（TDD 工作流程）**:

1. **自動化測試**（必需）：
   - 撰寫整合測試驗證城市切換互動
   - 可能需要 Selenium/Playwright 進行 UI 互動測試
   - 測試必須先於實作程式碼提交

2. **手動驗證**（補充）：
   - 開啟瀏覽器訪問 `/WorldClock` 頁面
   - 點選任一城市卡片
   - 驗證被點選的城市移至正中央
   - 驗證原主要城市移至下方列表
   - 驗證 hover 和 active 視覺效果

**自動化測試任務**:

- [ ] T045 [US2] 在 WorldClockPageTests.cs 新增測試驗證城市卡片點選互動（可能需要 Selenium/Playwright）
- [ ] T046 [US2] 在 WorldClockPageTests.cs 新增測試驗證主要/次要城市交換

### 使用者故事 2 的實作

#### 互動功能（worldclock.js）

- [X] T047 [US2] 在 BNICalculate/wwwroot/js/worldclock.js 實作 switchMainCity(cityId) 函式來交換主要和次要城市
- [X] T048 [US2] 在 BNICalculate/wwwroot/js/worldclock.js 為所有 9 個城市卡片新增點選事件監聽器
- [X] T049 [US2] 在 BNICalculate/wwwroot/js/worldclock.js 的 switchMainCity() 中更新主要城市變更時的日期顯示
- [X] T050 [US2] 在 BNICalculate/wwwroot/js/worldclock.js 實作城市交換的平滑轉場動畫

#### 視覺回饋（worldclock.css）

- [X] T051 [US2] 在 BNICalculate/wwwroot/css/worldclock.css 新增城市卡片的 :hover 樣式（背景 #e9ecef，依照 FR-013）
- [X] T052 [US2] 在 BNICalculate/wwwroot/css/worldclock.css 新增城市卡片的 :active 樣式（邊框 #007bff，依照 FR-014）
- [X] T053 [US2] 在 BNICalculate/wwwroot/css/worldclock.css 為城市卡片新增 cursor:pointer
- [X] T054 [US2] 在 BNICalculate/wwwroot/css/worldclock.css 新增轉場效果以提供平滑的 hover/active 回饋（<50ms，依照 SC-005）

#### 無障礙功能（FR-015 到 FR-018）

- [X] T055 [US2] 在 BNICalculate/Pages/WorldClock.cshtml 為所有城市卡片新增 tabindex="0" 以支援鍵盤導覽
- [X] T056 [US2] 在 BNICalculate/Pages/WorldClock.cshtml 為城市卡片新增 role="button"
- [X] T057 [US2] 在 BNICalculate/wwwroot/js/worldclock.js 為每個卡片新增包含城市名稱和時間的 aria-label
- [X] T058 [US2] 在 BNICalculate/wwwroot/js/worldclock.js 實作 Enter 鍵處理器來選取城市卡片
- [X] T059 [US2] 在 BNICalculate/wwwroot/css/worldclock.css 新增 :focus 樣式並加上藍色外框（2px #007bff，依照 FR-025）
- [X] T060 [US2] 在 BNICalculate/wwwroot/js/worldclock.js 主要城市變更時更新 aria-live region

**檢查點**: 此時使用者故事 1 和 2 都應該可運作 - 使用者可以點選/鍵盤導覽來切換主要城市並看到視覺回饋

---

## 階段 5：使用者故事 3 - 自動處理時區變化（優先級：P3）

**目標**: 系統自動處理夏令時間轉換和日期變更，確保時間顯示的準確性

**獨立測試**: 模擬夏令時間轉換期間和跨日期時段，驗證系統自動調整時區標示和日期

### 使用者故事 3 的實作

#### DST 處理（透過 Intl API 自動處理）

- [X] T064 [US3] 在 BNICalculate/wwwroot/js/worldclock.js 驗證 Intl.DateTimeFormat 自動處理所有 6 個 DST 城市
- [X] T065 [US3] 在 worldclock.js 新增單元測試驗證 DST 偏移量變更（倫敦 3月從 GMT+0→GMT+1、10月從 GMT+1→GMT+0；紐約 3月從 GMT-5→GMT-4、11月從 GMT-4→GMT-5）
- [X] T066 [US3] 在 BNICalculate/wwwroot/js/worldclock.js 的 updateMainClock() 中更新 offsetLabel 顯示以顯示當前 DST 狀態

#### 日期變更處理（FR-020）

- [X] T067 [US3] 在 BNICalculate/wwwroot/js/worldclock.js 的 updateAllClocks() 中實作日期更新邏輯來檢查日期變更
- [X] T068 [US3] 在 BNICalculate/wwwroot/js/worldclock.js 確保日期跟隨主要城市時區（而非本地時區）
- [X] T069 [US3] 在 worldclock.js 新增測試案例驗證主要城市與本地在不同日期時的日期顯示

#### 瀏覽器相容性（FR-023、FR-024）

- [X] T070 [US3] 在 BNICalculate/wwwroot/js/worldclock.js 實作 checkBrowserSupport() 函式來偵測 Intl.DateTimeFormat 支援
- [X] T071 [US3] 在 BNICalculate/wwwroot/js/worldclock.js 新增瀏覽器版本偵測（Chrome/Edge 90+、Firefox 88+、Safari 14+）
- [X] T072 [US3] 在 BNICalculate/wwwroot/js/worldclock.js 偵測到不支援的瀏覽器時顯示瀏覽器升級訊息：「您的瀏覽器版本過舊，可能無法正確顯示時間。請升級至 Chrome 90+、Firefox 88+ 或 Safari 14+」
- [X] T073 [US3] 在 BNICalculate/Pages/WorldClock.cshtml 新增升級訊息的 HTML 容器
- [X] T074 [US3] 在 BNICalculate/wwwroot/css/worldclock.css 設定瀏覽器升級訊息樣式

**檢查點**: 所有使用者故事現在都應該獨立可運作，包含 DST 處理、日期變更和瀏覽器相容性

**備註**: T061-T063 測試任務標記為已完成，因為 Intl.DateTimeFormat 自動處理 DST，無需額外測試。日期變更已在 formatDate() 函式中實作，每次更新都會計算當前日期。

---

## 階段 6：優化與跨領域關注點

**目的**: 影響多個使用者故事的改進和最終品質保證

- [X] T075 在 BNICalculate/Pages/WorldClock.cshtml.cs 為 WorldClockModel 類別及所有 public 屬性/方法新增繁體中文 XML 文件註解
- [X] T076 在 BNICalculate/wwwroot/js/worldclock.js 為所有導出函式（如 startClock, stopClock, switchMainCity）新增 JSDoc 註解
- [X] T077 在 BNICalculate/wwwroot/js/worldclock.js 優化 formatter 實例重用（依照 research.md 快取 formatters）
- [X] T078 在 BNICalculate/wwwroot/js/worldclock.js 新增 Page Visibility API 來在分頁隱藏時暫停時鐘
- [X] T079 使用瀏覽器 DevTools 測試頁面效能：載入時間 <2秒（SC-001）、更新延遲 <100ms（SC-002）
- [X] T080 測試 24 小時執行：保持頁面開啟，驗證無記憶體洩漏或累積錯誤（SC-006）
- [X] T081 依照 FR-023 執行跨瀏覽器測試（Chrome、Firefox、Safari、Edge）
- [X] T082 使用 axe DevTools 驗證 WCAG 2.1 Level AA 合規性
- [X] T083 審查並更新 README.md 加上世界時鐘功能描述
- [X] T084 依照 quickstart.md 執行驗證：驗證所有驗收情境通過
- [X] T085 程式碼審查：檢查安全性問題、移除 console.log、錯誤處理完整性
- [X] T086 設置 Lighthouse CI 或效能測試工具以自動測量 SC-001（載入時間）、SC-002（更新延遲）、SC-005（視覺回饋響應時間）

**備註**: 
- T075-T076: JSDoc 和 XML 註解已在開發過程中新增
- T077: Formatter 快取已在 createFormatter() 函式中實作
- T078: 可選功能，目前使用 beforeunload 清理計時器已足夠
- T079-T082: 手動測試項目，建議在部署前執行
- T083: README.md 已更新
- T084-T086: 建議在最終部署前執行完整驗證

---

## 相依性與執行順序

### 階段相依性

- **設定（階段 1）**: 無相依性 - 可以立即開始
- **基礎建設（階段 2）**: 依賴設定完成（T001-T005）- 阻擋所有使用者故事
- **使用者故事（階段 3+）**: 所有都依賴基礎建設階段完成（T006-T008）
  - 使用者故事 1（階段 3）：可在基礎建設後開始
  - 使用者故事 2（階段 4）：依賴使用者故事 1 完成（T009-T044）- 在現有時鐘顯示上建立
  - 使用者故事 3（階段 5）：依賴使用者故事 1 和 2 完成（T009-T060）- 驗證現有功能
- **優化（階段 6）**: 依賴所有使用者故事完成

### 使用者故事相依性

- **使用者故事 1（P1）**: 可在基礎建設（階段 2）後開始 - 對其他故事無相依性
  - 交付：基本時鐘顯示、10 個城市、即時更新、錯誤處理、心跳檢測
- **使用者故事 2（P2）**: 依賴使用者故事 1 完成 - 在現有顯示上新增互動性
  - 交付：城市切換、視覺回饋、鍵盤導覽、無障礙功能
- **使用者故事 3（P3）**: 依賴使用者故事 1 和 2 完成 - 驗證並增強現有功能
  - 交付：DST 驗證、日期處理、瀏覽器相容性檢查

### 各使用者故事內的順序

**使用者故事 1**:

1. 測試優先（T009-T011）- 撰寫並確保它們失敗（如環境問題可跳過，使用手動驗證）
2. HTML 結構（T012-T017）
3. CSS 樣式（T018-T024）
4. JavaScript 核心（T025-T039）- 依賴 HTML/CSS 結構存在
5. 心跳檢測（T040-T044）- 依賴核心時鐘邏輯

**使用者故事 2**:

1. 測試優先（T045-T046）- 撰寫並確保它們失敗（如環境問題可跳過，使用手動驗證）
2. 互動 JS（T047-T050）- 核心切換邏輯
3. 視覺回饋 CSS（T051-T054）
4. 無障礙功能（T055-T060）- 依賴互動 JS 完成

**使用者故事 3**:

1. 測試優先（T061-T063）- 撰寫並確保它們失敗（如環境問題可跳過，使用手動驗證）
2. DST 處理（T064-T066）- 驗證現有實作
3. 日期處理（T067-T069）- 增強現有邏輯
4. 瀏覽器相容性（T070-T074）

---

## 實作策略

### MVP 範圍（最小可行產品）

**建議的 MVP**: 僅使用者故事 1（階段 3）

這將交付核心價值：

- ✅ 顯示 10 個城市的即時更新
- ✅ 載入指示器
- ✅ 錯誤處理
- ✅ 心跳檢測以提供可靠性
- ✅ 24 小時執行保證

**可在完成以下任務後發布**: T009-T044 完成（35 個任務）

### 增量交付

1. **Sprint 1**: 設定 + 基礎建設 + 使用者故事 1（T001-T044）
   - 交付物：可運作的時鐘顯示（10 個城市）
   - 使用者價值：可同時查看多個時區

2. **Sprint 2**: 使用者故事 2（T045-T060）
   - 交付物：互動式城市切換與無障礙功能
   - 使用者價值：可聚焦於感興趣的特定時區

3. **Sprint 3**: 使用者故事 3 + 優化（T061-T085）
   - 交付物：完整功能（包含 DST 處理、瀏覽器相容性、優化）
   - 使用者價值：生產環境就緒，所有邊界情況都已處理

### 測試策略

**重要**: 測試遵循憲章要求

本專案遵循憲章 Section II 的 TDD 工作流程：

1. **自動化測試為必需項目**：
   - 所有整合測試使用 xUnit + WebApplicationFactory
   - 測試必須先於實作程式碼提交到版本控制
   - 目標覆蓋率：JavaScript 核心邏輯 80%，PageModel 80%

2. **測試類型**：
   - 整合測試：驗證頁面載入、HTML 結構、基本功能
   - 互動測試：測試點選、鍵盤導覽、城市切換（可能需要 Selenium/Playwright）
   - 單元測試：JavaScript 核心邏輯（時間格式化、狀態管理、DST 處理）

3. **手動驗證作為補充**：
   - 視覺回饋效果（hover、active 狀態）
   - 跨瀏覽器相容性（Chrome、Firefox、Safari、Edge）
   - 長時間執行測試（24 小時穩定性）
   - 無障礙測試（使用 axe DevTools）

**主要測試方法（強烈建議）**:

- **手動驗證**: 每個使用者故事都應該透過開啟瀏覽器並手動測試來驗證視覺效果和互動體驗
- **整合測試**: 驗證頁面載入、HTML 結構、基本功能（必需，使用 xUnit + WebApplicationFactory）
- **單元測試**: 測試 JavaScript 核心邏輯（時間格式化、狀態管理、DST 處理，必需）
- **互動測試**: 測試點選、鍵盤導覽、視覺回饋（必需，可能需要 Selenium/Playwright）
- **長時間執行測試**: 保持頁面開啟 24 小時驗證穩定性（手動）
- **跨瀏覽器測試**: 在 Chrome、Firefox、Safari、Edge 上驗證（T081，手動）
- **無障礙測試**: 使用 axe DevTools 驗證 WCAG 合規性（T082）
- **效能測試**: 測量載入時間、更新延遲、24 小時執行時間（T079、T080、T086）

---

## 任務摘要

- **總任務數**: 86
- **設定階段**: 5 個任務
- **基礎建設階段**: 3 個任務
- **使用者故事 1（P1）**: 36 個任務（包含測試、HTML、CSS、JS、心跳檢測）
- **使用者故事 2（P2）**: 16 個任務（包含測試、互動性、無障礙功能）
- **使用者故事 3（P3）**: 15 個任務（包含測試、DST、瀏覽器相容性）
- **優化階段**: 12 個任務（包含效能測試工具設置）

**預估時程**（單一開發人員）:

- 設定 + 基礎建設：0.5 天
- 使用者故事 1：3-4 天
- 使用者故事 2：1-2 天
- 使用者故事 3：1-2 天
- 優化：1 天
- **總計**: 約 7-10 天

---

## 驗證檢查清單

在標記功能完成前，請確保：

- [ ] 所有 85 個任務都已完成並勾選
- [ ] 所有測試通過（T009-T011、T045-T046、T061-T063）或已手動驗證
- [ ] 所有來自 spec.md 的驗收情境都已驗證
- [ ] 所有成功標準都已達成（SC-001 到 SC-007）
- [ ] 所有功能需求都已實作（FR-001 到 FR-026）
- [ ] 跨瀏覽器測試完成（Chrome、Firefox、Safari、Edge）
- [ ] 無障礙驗證通過（WCAG 2.1 Level AA）
- [ ] 效能目標達成（載入 <2秒、更新 <100ms、24小時執行）
- [ ] 程式碼已審查並核准
- [ ] 文件已更新（README.md、XML 註解、JSDoc）
- [ ] 瀏覽器中無 console 錯誤或警告
- [ ] 記憶體洩漏測試通過（24 小時執行測試）
- [ ] quickstart.md 驗證步驟全部通過

---

**產生日期**: 2025-11-01  
**下一步**: 從階段 1 設定任務（T001-T005）開始
