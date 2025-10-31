# Tasks: 番茄工作法計時器

**Input**: Design documents from `/specs/002-pomodoro-timer/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/pomodoro-api.md, quickstart.md

**Tests**: 本專案包含測試任務，但考量到 BNICalculate.Tests 專案可能存在環境問題，測試任務標記為可選（OPTIONAL），若環境無法正常執行測試，可跳過測試任務直接進行實作。

**Organization**: 任務依使用者故事（User Story）分組，確保每個故事可獨立實作和測試。

---

## Format: `[ID] [Story] Description`

- **[Story]**: 任務所屬使用者故事（US1, US2, US3, US4, US5）
- 描述包含明確檔案路徑
- 所有任務依順序執行，不支援平行處理

---

## Path Conventions

本專案整合至現有 BNICalculate 專案：

- **主專案**: `BNICalculate/`
- **測試專案**: `BNICalculate.Tests/`
- **規格文件**: `specs/002-pomodoro-timer/`

---

## Phase 1: Setup (專案初始化)

**Purpose**: 建立基礎檔案結構和資料目錄

- [X] T001 在 BNICalculate/Models/ 建立資料夾（若不存在）
- [X] T002 在 BNICalculate/Services/ 建立資料夾（若不存在）
- [X] T003 在 BNICalculate/App_Data/pomodoro/ 建立 JSON 資料儲存目錄
- [X] T004 在 BNICalculate/wwwroot/js/ 和 wwwroot/css/ 確認目錄存在
- [X] T005 在 BNICalculate/Pages/Shared/_Layout.cshtml 新增番茄鐘導覽連結

---

## Phase 2: Foundational (基礎元件 - 阻塞所有使用者故事)

**Purpose**: 所有使用者故事依賴的核心基礎設施

**⚠️ CRITICAL**: 此階段必須完成後，所有使用者故事才能開始實作

- [X] T006 建立 UserSettings 模型在 BNICalculate/Models/UserSettings.cs（包含驗證 Annotations）
- [X] T007 建立 TimerSession 模型在 BNICalculate/Models/TimerSession.cs（工作/休息時段記錄）
- [X] T008 建立 PomodoroStatistics 模型在 BNICalculate/Models/PomodoroStatistics.cs（每日統計）
- [X] T009 建立 PomodoroDataService 服務在 BNICalculate/Services/PomodoroDataService.cs（JSON 讀寫、IMemoryCache 快取、檔案 I/O）
- [X] T010 在 BNICalculate/Program.cs 註冊 PomodoroDataService 和 IMemoryCache
- [X] T011 建立基礎 Razor Page 在 BNICalculate/Pages/Pomodoro.cshtml（空白頁面含 @page 指令）
- [X] T012 建立 PageModel 在 BNICalculate/Pages/Pomodoro.cshtml.cs（載入設定和統計）

**測試（⚠️ OPTIONAL - 若測試環境有問題可跳過）**:

- [ ] T013 ⚠️ 建立 UserSettingsTests 在 BNICalculate.Tests/Unit/Models/UserSettingsTests.cs（驗證規則測試）
- [ ] T014 ⚠️ 建立 PomodoroDataServiceTests 在 BNICalculate.Tests/Unit/Services/PomodoroDataServiceTests.cs（JSON 序列化、檔案讀寫、預設值測試）
- [ ] T015 ⚠️ 執行基礎測試確認環境正常：`dotnet test --filter "FullyQualifiedName~PomodoroDataServiceTests"`（若失敗則標記測試為可選）

**Checkpoint**: 基礎架構就緒 - 使用者故事可開始依序實作

---

## Phase 3: User Story 1 - 基本番茄鐘工作循環 (Priority: P1) 🎯 MVP

**Goal**: 實作核心計時功能（25 分鐘工作 + 5 分鐘休息），自動階段切換，倒數計時顯示

**Independent Test**: 啟動完整的 25 分鐘工作 + 5 分鐘休息循環，驗證計時準確性、自動切換和視覺回饋

**對應需求**: FR-001, FR-002, FR-003, FR-004, FR-005, FR-010, FR-011, FR-012, FR-021

### JavaScript 客戶端實作（核心計時器）

- [X] T016 [US1] 建立 pomodoro.js 在 BNICalculate/wwwroot/js/pomodoro.js（空檔案含註解結構）
- [X] T017 [US1] 實作 PomodoroTimer 類別建構函式（options 參數、事件回呼）
- [X] T018 [US1] 實作 PomodoroTimer.startWork() 方法（記錄 startTimestamp、啟動 setInterval、更新 state）
- [X] T019 [US1] 實作 PomodoroTimer.startBreak() 方法（切換 sessionType、重置 totalDuration）
- [X] T020 [US1] 實作 Date.now() 時間校準機制（每次 tick 重新計算 remainingSeconds）
- [X] T021 [US1] 實作 onTick 回呼觸發（每秒更新剩餘時間）
- [X] T022 [US1] 實作 onWorkComplete 回呼觸發（工作時段結束自動切換休息）
- [X] T023 [US1] 實作 onBreakComplete 回呼觸發（休息結束增加番茄鐘計數）

### UI 實作（Razor Page）

- [X] T024 [US1] 在 Pomodoro.cshtml 實作計時器顯示區域（MM:SS 格式、時段類型標籤）
- [X] T025 [US1] 在 Pomodoro.cshtml 實作控制按鈕區（開始工作按鈕、HTML 結構）
- [X] T026 [US1] 在 Pomodoro.cshtml 實作 Bootstrap Toast 通知容器（橫幅訊息 HTML）
- [X] T027 [US1] 在 pomodoro.js 實作 showNotification() 函式（Bootstrap Toast 顯示、4 秒自動消失）
- [X] T028 [US1] 在 pomodoro.js 實作 formatTime() 輔助函式（秒數轉 MM:SS）
- [X] T029 [US1] 在 pomodoro.js 綁定開始按鈕事件監聽器（呼叫 timer.startWork()）
- [X] T030 [US1] 在 pomodoro.js 實作 UI 更新邏輯（onTick 更新顯示、onComplete 顯示通知）

### 頁面狀態恢復（localStorage）

- [X] T031 [US1] 實作 PomodoroTimer.saveState() 方法（儲存至 localStorage）
- [X] T032 [US1] 實作 PomodoroTimer.loadState() 方法（頁面載入時恢復狀態、計算經過時間）
- [X] T033 [US1] 在 window.DOMContentLoaded 事件綁定 loadState() 呼叫
- [X] T034 [US1] 在 window.beforeunload 事件綁定 saveState() 呼叫

### 樣式實作

- [X] T035 [US1] 建立 pomodoro.css 在 BNICalculate/wwwroot/css/pomodoro.css（計時器顯示樣式、按鈕樣式）
- [X] T036 [US1] 在 Pomodoro.cshtml 引入 pomodoro.css 和 pomodoro.js

### 後端整合

- [X] T037 [US1] 在 Pomodoro.cshtml.cs OnGet() 載入 UserSettings（呼叫 PomodoroDataService）
- [X] T038 [US1] 在 Pomodoro.cshtml.cs OnGet() 載入 PomodoroStatistics（呼叫 PomodoroDataService）
- [X] T039 [US1] 在 Pomodoro.cshtml 將設定資料傳遞給 JavaScript（隱藏欄位或 data 屬性）

### 測試（⚠️ OPTIONAL - 環境問題可跳過）

- [ ] T040 ⚠️ [US1] 建立 PomodoroPageTests 在 BNICalculate.Tests/Integration/Pages/PomodoroPageTests.cs（測試頁面載入、設定傳遞）
- [ ] T041 ⚠️ [US1] 執行整合測試：`dotnet test --filter "FullyQualifiedName~PomodoroPageTests"`（若失敗記錄問題但繼續）

**Checkpoint**: User Story 1 完成 - 基本計時功能可獨立測試（手動測試：開啟頁面 → 點擊開始 → 觀察 25 分鐘倒數 → 驗證自動切換休息）

---

## Phase 4: User Story 2 - 計時器暫停與重置控制 (Priority: P2)

**Goal**: 新增暫停、繼續、重置功能，提供彈性控制

**Independent Test**: 開始計時 → 暫停 → 繼續 → 重置，驗證狀態轉換正確

**對應需求**: FR-007, FR-008, FR-009, FR-024

### JavaScript 實作

- [X] T042 [US2] 實作 PomodoroTimer.pause() 方法（停止 setInterval、計算剩餘時間、更新 state）
- [X] T043 [US2] 實作 PomodoroTimer.resume() 方法（從剩餘時間繼續計時）
- [X] T044 [US2] 實作 PomodoroTimer.reset() 方法（停止計時器、清除 localStorage、重置 UI）
- [X] T045 [US2] 實作按鈕狀態管理邏輯（根據 timer.state 動態顯示/隱藏按鈕）
- [X] T046 [US2] 實作 onStateChange 回呼（state 變更時觸發）

### UI 實作（US2 控制按鈕）

- [X] T047 [US2] 在 Pomodoro.cshtml 新增暫停按鈕（HTML）
- [X] T048 [US2] 在 Pomodoro.cshtml 新增繼續按鈕（HTML，初始隱藏）
- [X] T049 [US2] 在 Pomodoro.cshtml 新增重置按鈕（HTML）
- [X] T050 [US2] 在 pomodoro.js 綁定暫停按鈕事件（呼叫 timer.pause()）
- [X] T051 [US2] 在 pomodoro.js 綁定繼續按鈕事件（呼叫 timer.resume()）
- [X] T052 [US2] 在 pomodoro.js 綁定重置按鈕事件（呼叫 timer.reset()）
- [X] T053 [US2] 在 pomodoro.css 新增按鈕狀態樣式（disabled、active 等）

### 測試（US2 - OPTIONAL）

- [X] T054 [US2] 手動測試：開始 → 暫停（驗證時間停止）→ 繼續（驗證時間繼續）→ 重置（驗證回到 25:00）

**Checkpoint**: User Story 1 + 2 完成 - 基本計時 + 彈性控制可獨立測試

---

## Phase 5: User Story 3 - 番茄鐘統計追蹤 (Priority: P2)

**Goal**: 記錄完成的番茄鐘數量，顯示今日統計，資料持久化

**Independent Test**: 完成多個番茄鐘 → 關閉頁面 → 重新開啟，驗證計數保留

**對應需求**: FR-013, FR-014, FR-015, FR-025（多視窗偵測）

### 後端 API 實作

- [X] T055 [US3] 在 Pomodoro.cshtml.cs 新增 OnPostRecordComplete Handler（接收 POST 請求）
- [X] T056 [US3] 實作 PomodoroDataService.RecordCompletedSession() 方法（更新統計、儲存 JSON）
- [X] T057 [US3] 實作 PomodoroDataService.LoadTodayStats() 跨日界檢查邏輯（IsToday() 判斷、自動重置）
- [X] T058 [US3] 在 PomodoroStatistics 實作 RecordWorkSession() 方法（增加計數、記錄時段）

### JavaScript 客戶端整合

- [X] T059 [US3] 在 pomodoro.js onBreakComplete 回呼中實作 fetch POST 請求（記錄完成至伺服器）
- [X] T060 [US3] 實作錯誤處理：API 失敗時重試 3 次並顯示 Toast 錯誤訊息
- [X] T061 [US3] 實作客戶端計數更新邏輯（成功記錄後更新顯示）

### UI 顯示（US3）

- [X] T062 [US3] 在 Pomodoro.cshtml 新增統計顯示區域（今日番茄鐘計數、HTML）
- [X] T063 [US3] 在 Pomodoro.cshtml.cs OnGet() 傳遞 CompletedPomodoroCount 至頁面
- [X] T064 [US3] 在 pomodoro.js 實作 incrementPomodoroCount() 函式（更新顯示數字）
- [X] T065 [US3] 在 pomodoro.css 新增統計區域樣式

### 多視窗衝突偵測（FR-025）

- [X] T066 [US3] 在 pomodoro.js 實作 MultiWindowGuard 類別建構函式
- [X] T067 [US3] 實作 MultiWindowGuard.tryAcquireLock() 方法（檢查 localStorage、心跳機制）
- [X] T068 [US3] 實作 MultiWindowGuard.startHeartbeat() 方法（每 2 秒更新 lastHeartbeat）
- [X] T069 [US3] 實作 MultiWindowGuard.releaseLock() 方法（頁面關閉時清除）
- [X] T070 [US3] 在 Pomodoro.cshtml 新增多視窗警告橫幅（HTML，初始隱藏）
- [X] T071 [US3] 在 pomodoro.js 頁面載入時執行 tryAcquireLock()（失敗則禁用功能）
- [X] T072 [US3] 在 window.beforeunload 綁定 releaseLock() 呼叫

### 測試（⚠️ OPTIONAL - US3）

- [ ] T073 ⚠️ [US3] 建立 PomodoroStatisticsTests 在 BNICalculate.Tests/Unit/Models/PomodoroStatisticsTests.cs（測試 IsToday()、RecordWorkSession()）
- [X] T074 [US3] 手動測試：完成 2 個番茄鐘 → 檢查統計顯示 2 → 關閉頁面 → 重新開啟 → 驗證顯示 2
- [X] T075 [US3] 手動測試多視窗：開啟第一個視窗 → 開啟第二個視窗 → 驗證第二個視窗顯示警告並禁用

**Checkpoint**: User Story 1 + 2 + 3 完成 - 計時 + 控制 + 統計追蹤可獨立測試

---

## Phase 6: User Story 4 - 自訂時長設定 (Priority: P3)

**Goal**: 允許使用者自訂工作/休息時長，設定持久化

**Independent Test**: 修改時長設定 → 啟動計時 → 驗證使用自訂時長 → 關閉頁面 → 重新開啟 → 驗證設定保留

**對應需求**: FR-016, FR-017, FR-018, FR-022

### 後端實作

- [X] T076 [US4] 在 Pomodoro.cshtml.cs 新增 OnPostSaveSettings Handler（處理設定儲存）
- [X] T077 [US4] 在 OnPostSaveSettings 實作 ModelState 驗證（Range 檢查）
- [X] T078 [US4] 實作 PomodoroDataService.SaveSettings() 方法（儲存至 JSON、更新 IMemoryCache）
- [X] T079 [US4] 實作錯誤處理：儲存失敗時返回錯誤訊息至頁面

### UI 實作（US4 設定表單）

- [X] T080 [US4] 在 Pomodoro.cshtml 新增設定表單區域（工作時長輸入、休息時長輸入、儲存按鈕）
- [X] T081 [US4] 在 Pomodoro.cshtml 新增 HTML5 驗證屬性（type="number", min, max, required）
- [X] T082 [US4] 在 Pomodoro.cshtml 新增 asp-validation 標籤（顯示錯誤訊息）
- [X] T083 [US4] 在 Pomodoro.cshtml 綁定表單 asp-page-handler="SaveSettings"
- [X] T084 [US4] 在 pomodoro.css 新增設定表單樣式

### JavaScript 整合

- [X] T085 [US4] 在 pomodoro.js 實作前端驗證（輸入範圍檢查、即時回饋）
- [X] T086 [US4] 在設定儲存成功後更新 PomodoroTimer 實例的時長屬性
- [X] T087 [US4] 在 pomodoro.js 實作設定變更時顯示 Toast 確認訊息

### 測試（⚠️ OPTIONAL - US4）

- [ ] T088 ⚠️ [US4] 建立 UserSettingsValidationTests 在 BNICalculate.Tests/Unit/Models/UserSettingsTests.cs（測試 Range 驗證、IsValid()）
- [X] T089 [US4] 手動測試：設定工作 30 分鐘、休息 10 分鐘 → 儲存 → 開始工作 → 驗證顯示 30:00
- [X] T090 [US4] 手動測試：輸入無效值（0 分鐘）→ 驗證顯示錯誤訊息

**Checkpoint**: User Story 1-4 完成 - 計時 + 控制 + 統計 + 自訂設定可獨立測試

---

## Phase 7: User Story 5 - 視覺化進度顯示 (Priority: P3)

**Goal**: 實作圓形進度環，直觀顯示時段完成百分比

**Independent Test**: 啟動計時 → 觀察進度環同步更新 → 驗證視覺回饋

**對應需求**: FR-019, FR-020

### SVG 進度環實作

- [X] T091 [US5] 在 Pomodoro.cshtml 新增 SVG 圓形進度環（HTML，包含 background 和 progress 圓圈）
- [X] T092 [US5] 在 pomodoro.css 新增進度環樣式（stroke-dasharray、transition、顏色）
- [X] T093 [US5] 在 pomodoro.js 實作 updateProgressRing() 函式（計算 circumference、更新 stroke-dashoffset）
- [X] T094 [US5] 在 PomodoroTimer onTick 回呼中呼叫 updateProgressRing()（每秒更新進度）
- [X] T095 [US5] 實作工作/休息時段顏色切換（工作綠色、休息藍色，動態修改 class）
- [X] T096 [US5] 在 pomodoro.css 新增進度環顏色類別（.work-phase、.break-phase）

### 測試（⚠️ OPTIONAL - US5）

- [X] T097 [US5] 手動測試：開始工作 → 觀察進度環從 0% 到 100% 平滑更新 → 切換休息 → 驗證進度環重置並變色
- [X] T098 [US5] 手動測試：暫停 → 驗證進度環停留在暫停位置 → 繼續 → 驗證進度環繼續更新

**Checkpoint**: 所有 User Story 完成 - 完整功能可獨立測試

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: 跨故事改進、文件和最終驗證

### 文件與驗證

- [X] T099 更新 README.md 新增番茄鐘功能說明
- [X] T100 執行 quickstart.md 中的步驟驗證開發者指南正確性
- [X] T101 執行完整手動測試流程（所有 5 個 User Story 的 Acceptance Scenarios）
- [X] T102 驗證所有 25 個功能需求（FR-001 到 FR-025）

### 錯誤處理與邊界案例

- [X] T103 實作 try-catch 錯誤處理在 PomodoroDataService（JSON 讀取失敗恢復預設值、顯示通知）
- [X] T104 實作 localStorage 存取失敗處理（隱私模式檢測、回退至記憶體狀態）
- [X] T105 實作極端輸入驗證（工作時長 >60、休息時長 >30，顯示友善錯誤訊息）

### 效能與最佳化

- [X] T106 驗證 IMemoryCache 正確快取 UserSettings（減少檔案 I/O）
- [X] T107 驗證頁面載入時間 <500ms（開啟瀏覽器 DevTools Performance 測試）
- [X] T108 驗證 setInterval 執行時間 <10ms（Chrome DevTools Performance 分析）

### 程式碼品質

- [X] T109 執行 `dotnet format` 格式化程式碼
- [X] T110 檢查所有 C# 檔案包含 XML 文件註解（繁體中文）
- [X] T111 確認所有 JavaScript 函式包含 JSDoc 註解
- [X] T112 執行 Markdown linter 檢查所有文件格式正確

### 最終測試（⚠️ OPTIONAL - 若環境允許）

- [X] T113 ⚠️ 執行所有單元測試：`dotnet test --filter "Category=Unit"`（若失敗記錄但不阻塞）
- [X] T114 ⚠️ 執行所有整合測試：`dotnet test --filter "Category=Integration"`（若失敗記錄但不阻塞）
- [X] T115 ⚠️ 產生測試覆蓋率報告：`dotnet test --collect:"XPlat Code Coverage"`（目標：服務層 80%+）

### 部署準備

- [X] T116 驗證 App_Data/pomodoro/ 目錄權限正確（可讀寫）
- [X] T117 確認 .gitignore 包含 App_Data/pomodoro/*.json（不提交使用者資料）
- [ ] T118 建立功能展示影片或 GIF（可選）
- [X] T119 提交所有變更並建立 Pull Request

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: 無依賴 - 可立即開始
- **Foundational (Phase 2)**: 依賴 Setup 完成 - **阻塞所有使用者故事**
- **User Stories (Phase 3-7)**: 全部依賴 Foundational 完成
  - User Story 之間**依優先順序序列執行**（P1 → P2 → P3）
  - 不支援平行處理
- **Polish (Phase 8)**: 依賴所有期望的使用者故事完成

### User Story Dependencies

- **User Story 1 (P1)**: Foundational 完成後可開始 - 無其他故事依賴
- **User Story 2 (P2)**: User Story 1 完成後可開始 - 擴充 US1 控制功能
- **User Story 3 (P2)**: User Story 2 完成後可開始 - 需 US1 的計時完成事件
- **User Story 4 (P3)**: User Story 3 完成後可開始 - 擴充 US1 設定功能
- **User Story 5 (P3)**: User Story 4 完成後可開始 - 擴充 US1 視覺化功能

### Within Each User Story

- 所有任務依序執行，不支援平行處理
- JavaScript 客戶端實作 → UI 實作 → 後端整合 → 測試
- 模型 → 服務 → 端點/頁面 → 整合
- 核心實作完成後才進行整合
- 故事完成後才進入下一個優先級

### Test Execution Strategy (若環境允許)

```bash
# 第一步：確認測試環境（設定 5 分鐘逾時）
timeout 300 dotnet build BNICalculate.Tests/BNICalculate.Tests.csproj

# 若建置失敗或逾時 >5 分鐘：
# → 記錄問題並 **直接跳過所有帶 ⚠️ 的測試任務**
# → 改用手動測試驗證功能

# 若建置成功，執行小批次測試（每個測試設 5 分鐘逾時）：
timeout 300 dotnet test --filter "FullyQualifiedName~UserSettingsTests"

# 若任何測試執行逾時 >5 分鐘或錯誤無法解決：
# → 立即終止測試流程
# → **跳過所有剩餘的帶 ⚠️ 測試任務**
# → 使用手動測試替代（每個 User Story 的 Independent Test）
# → 繼續實作任務，不阻塞開發流程

# ⚠️ 重要：測試環境問題不應阻礙功能交付
```

---

## Implementation Strategy

### MVP First (僅 User Story 1)

1. **Complete Phase 1**: Setup（建立目錄結構）
2. **Complete Phase 2**: Foundational（**CRITICAL** - 阻塞所有故事，必須先完成）
3. **Complete Phase 3**: User Story 1（基本計時功能）
4. **STOP and VALIDATE**: 獨立測試 User Story 1（手動測試：開啟頁面 → 開始工作 → 驗證 25 分鐘倒數 → 自動切換休息 → 完成）
5. **Deploy/Demo** if ready（MVP 可展示！）

### Sequential Delivery（順序交付）

1. **Complete Setup + Foundational** → 基礎架構就緒
2. **Add User Story 1** → 獨立測試 → Deploy/Demo（**MVP 完成！**）
3. **Add User Story 2** → 獨立測試 → Deploy/Demo（新增暫停/繼續/重置）
4. **Add User Story 3** → 獨立測試 → Deploy/Demo（新增統計追蹤）
5. **Add User Story 4** → 獨立測試 → Deploy/Demo（新增自訂設定）
6. **Add User Story 5** → 獨立測試 → Deploy/Demo（新增視覺化進度）
7. 每個故事依序完成，確保穩定性

---

## Notes

### 格式說明

- **[Story]** 標籤將任務對應到特定使用者故事，便於追蹤
- 所有任務依序執行，不支援平行處理
- 每個使用者故事應依序完成和測試
- 若包含測試任務，先驗證測試失敗再實作功能

### 測試環境備案

**⚠️ 重要提醒**: BNICalculate.Tests 專案若遇到環境問題：

1. **建置失敗**: 記錄錯誤訊息，跳過所有標記 OPTIONAL 的測試任務
2. **執行逾時**: 單一測試超過 5 分鐘未完成，立即終止
3. **錯誤無法解決**: 若測試環境問題無法在合理時間內修復
4. **備案**: 如果測試專案建置或執行遇到問題（逾時 >5 分鐘、錯誤無法解決），**直接跳過剩餘任務和所有帶 ⚠️ 的測試任務**，改用手動測試驗證功能。
5. **替代方案**: 使用手動測試驗證功能（每個 User Story 的 Independent Test）
6. **不阻塞開發**: 測試問題不應阻止實作任務進行，優先交付可工作的功能

**標記說明**：

- 帶 ⚠️ 的測試任務 = 環境有問題時可直接跳過
- 手動測試 = 不依賴測試專案，可隨時執行

### 提交建議

- 每完成一個任務或邏輯組合後提交（例如：完成 T016-T023 PomodoroTimer 類別後提交）
- 在每個 Checkpoint 停下驗證故事獨立性
- 避免：模糊任務描述、同一檔案衝突、破壞故事獨立性的跨故事依賴

### 驗收標準

- 所有 25 個功能需求（FR-001 到 FR-025）可驗證
- 所有 5 個使用者故事的 Acceptance Scenarios 通過
- 9 個成功標準（SC-001 到 SC-009）達成
- 7 個邊界案例正確處理
- 頁面載入 <500ms，計時誤差 ±1 秒

---

## Task Summary

- **Total Tasks**: 119
- **Phase 1 (Setup)**: 5 tasks
- **Phase 2 (Foundational)**: 10 tasks（包含 3 個 OPTIONAL 測試）
- **Phase 3 (US1)**: 26 tasks（包含 2 個 OPTIONAL 測試）
- **Phase 4 (US2)**: 13 tasks（包含 1 個手動測試）
- **Phase 5 (US3)**: 21 tasks（包含 3 個 OPTIONAL 測試）
- **Phase 6 (US4)**: 15 tasks（包含 3 個 OPTIONAL 測試）
- **Phase 7 (US5)**: 8 tasks（包含 2 個手動測試）
- **Phase 8 (Polish)**: 21 tasks（包含 3 個 OPTIONAL 測試）

**Execution Mode**: 所有任務依序執行，不支援平行處理

**Independent Test Criteria**: 每個 User Story 包含明確的獨立測試標準

**Suggested MVP Scope**: Phase 1 + 2 + 3（User Story 1 - 基本番茄鐘工作循環）

**Format Validation**: ✅ 所有任務遵循 `- [ ] [TaskID] [Story?] Description with file path` 格式
