# Research & Technical Decisions: BMI 計算器

**Feature**: 001-bmi-calculator  
**Date**: 2025年10月31日  
**Purpose**: 解決技術選擇和實作方法的不確定性

## 研究任務總覽

由於此功能範圍簡單且技術堆疊明確（ASP.NET Core 8.0 Razor Pages），大部分技術決策已在 Technical Context 中確定。以下記錄關鍵決策和最佳實務。

---

## 決策 1: 客戶端 vs 伺服器端計算

### 背景

BMI 計算可以在伺服器端（C# PageModel）或客戶端（JavaScript）執行。

### 選項評估

| 選項 | 優點 | 缺點 |
|------|------|------|
| **伺服器端計算** | - 集中邏輯管理<br>- 易於單元測試（C#） | - 需要表單提交和頁面重新載入<br>- 增加伺服器負載<br>- 使用者體驗較差（延遲） |
| **客戶端計算** | - 即時回饋，無需頁面重新整理<br>- 零伺服器負載<br>- 符合「簡單」原則<br>- 可離線使用 | - 需要 JavaScript<br>- 需要瀏覽器端測試 |

### 決策

✅ **選擇：客戶端計算（JavaScript）**

### 理由

1. **使用者體驗**：SC-002 要求「1 秒內完成計算」，客戶端即時計算可達成 <100ms
2. **效能**：無伺服器往返，減少延遲
3. **簡潔性**：符合「超簡易」和「簡潔樸素」原則
4. **範圍適合**：BMI 計算是純函數，無狀態依賴，非常適合客戶端執行

### 實作方法

- 使用純 JavaScript（無 jQuery 或其他函式庫）
- 監聽 input 事件進行即時驗證
- 點擊「計算」按鈕時觸發計算和顯示結果

---

## 決策 2: 表單驗證策略

### 背景

需要驗證身高和體重輸入（正數、數字格式、非空）。

### 選項評估

| 選項 | 優點 | 缺點 |
|------|------|------|
| **HTML5 驗證** | 瀏覽器原生支援 | 驗證訊息英文，無法自訂中文訊息 |
| **JavaScript 驗證** | 完全控制錯誤訊息（繁體中文）<br>可自訂顯示位置和樣式 | 需額外程式碼 |
| **ASP.NET 驗證** | 伺服器端驗證安全 | 需表單提交，不符合客戶端計算策略 |

### 決策

✅ **選擇：JavaScript 驗證 + HTML5 輔助屬性**

### 理由

1. **語言需求**：憲章要求 UI 使用繁體中文，HTML5 驗證訊息為英文
2. **UX 需求**：規格要求「欄位下方顯示紅色行內錯誤訊息」
3. **即時性**：可在使用者輸入時即時驗證並提供回饋

### 實作方法

- 使用 `<input type="number">` 提供基本驗證
- JavaScript 監聽 `input` 和 `blur` 事件
- 在欄位下方動態插入/移除錯誤訊息 `<span class="error-message">`
- 驗證規則：
  - 非空值（required）
  - 數字格式（使用 `isNaN()` 檢查）
  - 正數（value > 0）

---

## 決策 3: CSS 架構

### 背景

規格要求「簡潔樸素，無特效或華麗裝飾」，不使用 CSS 框架。

### 選項評估

| 選項 | 優點 | 缺點 |
|------|------|------|
| **Bootstrap** | 快速開發，響應式 | 違反「簡潔樸素」原則，檔案過大 |
| **Tailwind CSS** | Utility-first，可客製化 | 學習曲線，違反「簡單」原則 |
| **自訂 CSS** | 完全控制，最小化檔案大小<br>符合「簡潔樸素」原則 | 需手寫 CSS |

### 決策

✅ **選擇：自訂 CSS（`bmi.css`）**

### 理由

1. **憲章合規**：符合「簡潔樸素」和「CSS <5KB」要求
2. **範圍適合**：單一頁面，元素少（標題、2 個輸入欄位、2 個按鈕、結果區域）
3. **效能**：無外部依賴，載入速度快

### 實作方法

- 建立 `wwwroot/css/bmi.css`
- 使用簡單的 Flexbox 或 Grid 佈局
- 基本樣式：
  - 置中容器（max-width: 500px）
  - 輸入欄位間距和邊框
  - 按鈕樣式（無圓角特效）
  - 錯誤訊息紅色文字
  - 結果區域綠色邊框

---

## 決策 4: 測試策略

### 背景

憲章要求 TDD 和 80% 覆蓋率，但此功能主要邏輯在客戶端 JavaScript。

### 選項評估

| 測試類型 | 工具 | 適用範圍 |
|----------|------|----------|
| **整合測試** | xUnit + WebApplicationFactory | Razor Page 渲染、HTML 結構 |
| **JavaScript 單元測試** | Jest / Jasmine | BMI 計算邏輯 |
| **E2E 測試** | Selenium / Playwright | 完整使用者流程 |

### 決策

✅ **選擇：整合測試（xUnit）+ 手動 JavaScript 測試**

### 理由

1. **範圍適合**：JavaScript 邏輯簡單（單一計算函數），手動測試即可
2. **成本效益**：設置 Jest 對此小型功能過度工程化
3. **憲章合規**：整合測試覆蓋 Razor Page 渲染和 HTML 結構

### 實作方法

- **整合測試** (`BMIPageTests.cs`):
  - 驗證頁面載入成功（HTTP 200）
  - 驗證 HTML 包含必要元素（輸入欄位、按鈕、結果區域）
  - 驗證輸入欄位屬性（type, name, id）
  - 驗證 JavaScript 檔案載入（script tag）
- **JavaScript 測試**:
  - 手動測試或未來可新增 Jest
  - 測試案例記錄在 `quickstart.md`

---

## 決策 5: 頁面路由

### 背景

決定 BMI 計算器的 URL 路徑。

### 選項評估

| 選項 | URL | 說明 |
|------|-----|------|
| **首頁** | `/` | 將 BMI 設為首頁 |
| **專用路徑** | `/BMI` | 獨立頁面路徑 |
| **功能路徑** | `/Calculator/BMI` | 分類路徑 |

### 決策

✅ **選擇：專用路徑 `/BMI`**

### 理由

1. **清晰性**：URL 明確表達功能
2. **擴展性**：未來可新增其他計算器（如 `/BMR`）
3. **不干擾現有首頁**：保留 `Index.cshtml` 作為專案首頁

### 實作方法

- 建立 `Pages/BMI.cshtml` 和 `Pages/BMI.cshtml.cs`
- Razor Pages 預設路由為 `/BMI`

---

## 技術堆疊總結

| 層級 | 技術 | 版本 | 說明 |
|------|------|------|------|
| **Runtime** | .NET | 8.0 | 既有專案框架 |
| **Web Framework** | ASP.NET Core Razor Pages | 8.0 | 既有專案類型 |
| **前端** | HTML5 + CSS3 + Vanilla JavaScript | - | 無框架，原生實作 |
| **測試** | xUnit + WebApplicationFactory | Latest | .NET 標準測試框架 |
| **IDE** | Visual Studio Code / Visual Studio | - | 既有開發環境 |

---

## 最佳實務參考

### ASP.NET Core Razor Pages

- [官方文件](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/)
- [Razor 語法參考](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor)

### JavaScript 表單驗證

- [MDN: 客戶端表單驗證](https://developer.mozilla.org/en-US/docs/Learn/Forms/Form_validation)
- [HTML5 Input Types](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input)

### 測試

- [ASP.NET Core 整合測試](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [xUnit 文件](https://xunit.net/)

---

## 風險與緩解措施

| 風險 | 影響 | 機率 | 緩解措施 |
|------|------|------|----------|
| JavaScript 在舊瀏覽器不支援 | 功能無法使用 | 低 | 使用 ES5 語法，避免 ES6+ 特性 |
| 使用者停用 JavaScript | 功能無法使用 | 低 | 顯示提示訊息「需要啟用 JavaScript」 |
| 輸入驗證繞過 | 顯示錯誤結果 | 低 | JavaScript 驗證已足夠（無安全風險） |

---

## 決策總結

所有 NEEDS CLARIFICATION 項目已解決：

1. ✅ 客戶端計算（JavaScript）
2. ✅ JavaScript 驗證 + HTML5 輔助
3. ✅ 自訂 CSS（無框架）
4. ✅ 整合測試（xUnit）+ 手動 JS 測試
5. ✅ 路徑 `/BMI`

**下一步**: 進入 Phase 1 - 設計資料模型和快速開始指南
