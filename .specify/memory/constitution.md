<!--
同步影響報告
============
版本變更: 1.1.0 → 1.2.0
修改的原則:
  - I. 程式碼品質標準 - 強化 .NET 特定要求（Nullable 參考型別、async/await 模式）
  - II. 測試標準 - 新增 .NET 測試架構指引（xUnit、MSTest）
  - IV. 效能要求 - 調整為 ASP.NET Core 最佳實務
新增區段:
  - 效能標準區段新增 .NET 特定工具和最佳化策略
  - 品質關卡區段更新為 .NET 建構管線要求
移除區段: 無
範本狀態:
  ✅ plan-template.md - 已驗證（憲章檢查對齊）
  ✅ spec-template.md - 已驗證（非函式需求對齊）
  ✅ tasks-template.md - 已驗證（TDD 工作流程對齊）
  ⚠ checklist-template.md - 待更新（需新增 .NET 特定檢查項目）
  ⚠ agent-file-template.md - 待審查（確認對齊性）
後續待辦事項:
  - 建立 .editorconfig 檔案定義程式碼風格規則
  - 設定 Directory.Build.props 用於集中式專案設定
關鍵變更: 針對 ASP.NET Core Razor Pages 專案優化憲章，明確 .NET 8.0 生態系統的工具、架構和最佳實務
版本說明: 使用 1.2.0 (MINOR) 因為這是對現有原則的重大擴充和 .NET 特定指引的新增
-->

# toolTest 專案憲章

**專案類型**: ASP.NET Core Razor Pages Web Application  
**目標框架**: .NET 8.0  
**開發環境**: Visual Studio Code / Visual Studio 2022+

## 核心原則

### I. 程式碼品質標準（不可協商）

所有程式碼在合併前必須符合以下品質標準：

- **靜態分析**：
  - 零編譯器警告（`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`）
  - 啟用 Nullable 參考型別（`<Nullable>enable</Nullable>`），所有可為 null 的參考型別必須正確標註和處理
  - 使用 Roslyn 分析器（建議：Microsoft.CodeAnalysis.NetAnalyzers）
  - Code Analysis 規則集維持在 "All Rules" 或 "Recommended" 等級
  
- **程式碼風格**：
  - 遵循 `.editorconfig` 定義的程式碼風格（縮排、命名慣例、換行規則）
  - 使用有意義的英文變數/方法/類別名稱（遵循 C# 命名慣例：PascalCase for public, camelCase for private）
  - 避免 "magic numbers" 和 "magic strings"，使用常數或設定檔
  - Async 方法必須以 `Async` 結尾（例如：`GetDataAsync`）
  
- **SOLID 原則**：
  - 單一職責原則：每個類別應該只有一個變更的理由
  - 使用相依性注入（DI）以提升可測試性和解耦
  - 介面優於具體實作（在 `Program.cs` 中註冊服務時使用介面）
  - 避免過度設計：僅在需要時才引入抽象層
  
- **文件撰寫**：
  - 公開 API（public classes, methods, properties）必須使用 XML 文件註解（`///`）
  - XML 註解建議使用繁體中文，但接受英文
  - 複雜商業邏輯需要行內註解說明「為什麼」而非「做什麼」
  - README.md 和使用者文件必須使用繁體中文
  
- **程式碼審查**：
  - 所有變更都需要至少一位同儕審查
  - 禁止自我合併（self-merge）
  - PR 描述必須包含：變更理由、測試方法、影響範圍
  - 審查者必須驗證憲章合規性

**理由**：一致的程式碼品質可減少技術債、改善可維護性，並實現團隊延展性。.NET 生態系統提供了強大的工具鏈（Roslyn 分析器、EditorConfig、Nullable 參考型別），善用這些工具可在編譯時期發現問題，而非執行時期。程式碼品質不良會隨時間累積，且修復難度呈指數增長。

### II. 測試標準（不可協商）

所有功能必須採用測試驅動開發（TDD）：

- **TDD 工作流程**：
  1. 撰寫失敗測試（Red）→ 取得程式碼審查核准 → 實作功能 → 測試通過（Green）→ 重構（Refactor）
  2. 測試必須先於實作程式碼提交到版本控制
  
- **測試架構**：
  - **單元測試**：使用 xUnit（建議）或 MSTest
  - **整合測試**：使用 WebApplicationFactory&lt;Program&gt; 進行端到端測試
  - **UI 測試**：使用 Selenium 或 Playwright 進行關鍵使用者旅程測試
  - **Mocking**：使用 Moq 或 NSubstitute 模擬相依性
  
- **覆蓋率要求**：
  - 商業邏輯（Services、Models）最低 **80%** 程式碼覆蓋率
  - 關鍵路徑（付款、驗證、資料完整性）**100%** 覆蓋率
  - 使用 Coverlet 或 dotnet-coverage 工具測量覆蓋率
  - 建構管線（CI）必須在覆蓋率低於門檻時失敗
  
- **必要的測試類型**：
  - **單元測試**：所有服務類別（`Services/`）、模型邏輯、輔助函式
  - **整合測試**：Razor Pages PageModel、JSON 檔案讀寫操作、外部 API 呼叫
  - **UI 測試**：關鍵使用者旅程（註冊、登入、核心功能流程）
  - **回歸測試**：每個修復的 bug 都必須有對應的測試防止再次發生
  
- **測試品質**：
  - 測試必須**獨立**（可單獨執行、不依賴執行順序）
  - 測試必須**可重複**（每次執行結果相同）
  - 測試必須**快速**（每個單元測試 <1 秒，整合測試 <5 秒）
  - 使用臨時測試 JSON 檔案或記憶體中的資料結構進行資料存取測試
  
- **測試命名與結構**：
  - 測試專案命名：`[ProjectName].Tests`（例如：`toolTest.Tests`）
  - 測試類別命名：`[TestedClass]Tests`（例如：`UserServiceTests`）
  - 測試方法命名：使用 `MethodName_Scenario_ExpectedResult` 模式
  - 測試結構：遵循 **Arrange-Act-Assert (AAA)** 模式

**測試專案結構範例**：

```text
toolTest.Tests/
├── Unit/
│   ├── Services/
│   │   └── UserServiceTests.cs
│   └── Models/
│       └── UserModelTests.cs
├── Integration/
│   ├── Pages/
│   │   └── IndexPageTests.cs
│   └── Data/
│       └── JsonDataAccessTests.cs
└── UI/
    └── UserJourneyTests.cs
```

**理由**：測試是活文件和安全網。TDD 確保可測試的設計並防止回歸錯誤。在 .NET 生態系統中，xUnit 和 WebApplicationFactory 提供了強大且易用的測試基礎設施。測試投資可減少除錯時間和正式環境事件，長期來看大幅降低維護成本。

### III. 使用者體驗一致性

所有面向使用者的功能必須維持一致的使用者體驗：

- **設計系統**：使用 `wwwroot/lib` 和 `Shared/` 版面配置中的共享 UI 元件
- **響應式設計**：行動優先方法；在行動裝置、平板電腦、桌面視窗測試
- **無障礙設計**：符合 WCAG 2.1 Level AA；語意化 HTML；支援鍵盤導覽
- **錯誤處理**：使用者友善的錯誤訊息；驗證回饋；不暴露技術術語
- **載入狀態**：對於 >300ms 的操作顯示載入指示器；提交期間停用按鈕
- **一致性**：所有頁面的表單、按鈕、導覽、回饋使用相同模式

**理由**：不一致的 UX 造成使用者困惑、增加支援成本並損害產品信譽。使用者期望整個應用程式具有可預測的行為。

### IV. 效能要求

效能目標在開發時強制執行，並在 CI/CD 管線中驗證：

- **頁面載入效能**：
  - 初始頁面載入（含伺服器端渲染）<2 秒（p95，3G 網路）
  - 後續導覽（SPA 風格或局部更新）<500 毫秒（p95）
  - Time to Interactive (TTI) <3 秒
  - Largest Contentful Paint (LCP) <2.5 秒
  
- **API 回應時間**：
  - GET 請求（讀取操作）<200 毫秒（p95）
  - POST/PUT 請求（寫入操作）<500 毫秒（p95）
  - JSON 檔案讀取操作 <100 毫秒
  - 複雜資料處理或聚合運算 <2 秒（考慮非同步處理或背景作業）
  
- **資料存取最佳化（JSON 檔案）**：
  - 使用非同步檔案讀寫（`File.ReadAllTextAsync()` 和 `File.WriteAllTextAsync()`）
  - 大型資料集（>100 項目）必須使用分頁處理
  - 實作快取機制避免頻繁讀取檔案（使用 `IMemoryCache`）
  - 使用 `System.Text.Json` 進行高效能 JSON 序列化/反序列化
  
- **記憶體管理**：
  - 應用程式正常負載下記憶體佔用 <500MB（監控 Working Set）
  - 避免記憶體洩漏：正確實作 `IDisposable` 並使用 `using` 陳述式
  - 大型物件使用串流處理而非一次載入記憶體
  - 使用 `ObjectPool` 重用昂貴物件（如 HttpClient、資料庫連線）
  
- **客戶端資源**：
  - JavaScript 打包檔案 <200KB（gzip 壓縮後）
  - CSS 檔案 <50KB（gzip 壓縮後）
  - 圖片最佳化：使用 WebP 格式，適當尺寸，lazy loading
  - 啟用靜態檔案壓縮（Brotli 或 Gzip）
  
- **ASP.NET Core 特定最佳化**：
  - 使用 Response Caching 中介軟體快取靜態內容
  - 使用 Output Caching（.NET 8 新功能）或 Memory Cache 快取頻繁存取的資料
  - 所有 I/O 操作使用 async/await（資料庫、檔案、HTTP 呼叫）
  - 啟用 HTTP/2 或 HTTP/3 以支援多工處理
  - 使用 `IMemoryCache` 或分散式快取（Redis）降低資料庫負載
  
- **監控與遙測**：
  - 整合 Application Insights 或 Prometheus + Grafana
  - 所有端點配備遙測（回應時間、錯誤率、相依性持續時間）
  - 在 CI 中捕捉效能回歸（使用 BenchmarkDotNet 或效能測試）
  - 設定警報：p95 回應時間超過目標值、錯誤率 >1%、記憶體使用異常

**理由**：效能是一項功能，而非事後考量。緩慢的應用程式會失去使用者並增加基礎設施成本。ASP.NET Core 提供了優秀的效能基準，但需要開發人員遵循最佳實務（async/await、快取、資料庫優化）才能實現。設定明確目標可實現客觀測量並防止效能隨時間降低。監控讓我們能夠早期發現問題，在影響使用者前解決。

### V. 文件與溝通語言

所有面向使用者和開發團隊的文件必須使用繁體中文：

- **功能規範**：所有功能需求文件（spec.md）必須使用繁體中文撰寫
- **實作計畫**：技術設計文件（plan.md）必須使用繁體中文撰寫
- **任務清單**：開發任務描述（tasks.md）必須使用繁體中文撰寫
- **使用者介面**：所有 UI 文字、錯誤訊息、表單標籤使用繁體中文
- **API 文件**：端點描述、參數說明使用繁體中文
- **註解規範**：程式碼註解可使用英文或繁體中文，但公開 API 的 XML 文件註解建議使用繁體中文

**例外**：程式碼變數名稱、函式名稱、類別名稱等識別符號使用英文命名以符合程式語言慣例。

**理由**：使用團隊母語撰寫文件可提升溝通效率、減少誤解，並降低新成員的學習曲線。一致的語言使用確保所有團隊成員都能充分理解需求和設計決策。

## 效能標準

### 測量與工具

- **效能分析（Profiling）**：
  - 使用 Visual Studio Profiler 或 dotnet-trace 進行 CPU 和記憶體分析
  - 使用 dotnet-counters 監控執行時期計數器（GC、執行緒、例外狀況）
  - 使用 BenchmarkDotNet 進行微基準測試和演算法比較
  - 監控檔案 I/O 操作的頻率和耗時
  
- **負載測試**：
  - 使用 Apache JMeter、k6、或 Azure Load Testing 模擬真實負載
  - 正式環境部署前必須進行負載測試（模擬預期流量的 150%）
  - 測試場景包含：正常負載、尖峰負載、持續負載、壓力測試
  - 驗證在負載下無記憶體洩漏（記憶體使用趨於穩定）
  
- **應用程式效能監控（APM）**：
  - 正式環境必須部署 Application Insights（Azure）或同等級 APM 解決方案
  - 追蹤端到端交易時間、相依性呼叫、例外狀況
  - 設定自訂遙測事件追蹤關鍵商業流程
  - 設定 Smart Detection 或自訂警報
  
- **CI 效能關卡**：
  - 建構管線中執行自動化效能測試（使用 BenchmarkDotNet 或整合測試）
  - 拒絕效能回歸 >10% 的 Pull Request
  - 追蹤效能趨勢（將結果發佈到儀表板）
  - 測試關鍵路徑的回應時間（登入、主頁載入、資料查詢）

### .NET 特定優化優先順序

1. **資料存取優化**（影響最大）：
   - 使用非同步檔案操作避免阻塞執行緒（`File.ReadAllTextAsync()` / `WriteAllTextAsync()`）
   - 實作記憶體快取減少檔案讀取次數（使用 `IMemoryCache`）
   - 使用 `System.Text.Json` 的高效能序列化選項（如 `JsonSerializerOptions`）
   - 批次處理資料變更，一次性寫入檔案而非多次寫入
   - 對於大型 JSON 檔案，考慮使用串流讀取（`JsonSerializer.DeserializeAsync` 搭配 `Stream`）

2. **快取策略**：
   - Response Caching：快取整個 HTTP 回應（適用靜態或半靜態頁面）
   - Memory Caching：使用 `IMemoryCache` 快取應用程式資料
   - Distributed Caching：使用 Redis 在多伺服器間共享快取
   - Output Caching (.NET 8+)：細粒度的頁面片段快取
   - 設定適當的快取過期策略（時間型或相依性型）

3. **非同步程式設計**：
   - 所有 I/O 操作使用 async/await（資料庫、檔案、HTTP 呼叫）
   - 避免 `Task.Wait()` 或 `.Result`（會導致死鎖）
   - 使用 `ConfigureAwait(false)` 在函式庫程式碼中避免不必要的執行緒切換
   - 善用 `IAsyncEnumerable<T>` 處理大型資料集串流

4. **靜態檔案與客戶端優化**：
   - 啟用 Response Compression（Brotli 優先，Gzip 備用）
   - 使用 CDN 服務靜態資源（wwwroot）
   - 設定適當的 Cache-Control 標頭（長期快取不變資源）
   - 壓縮和最小化 JavaScript/CSS（使用建置時期工具）
   - 使用 Bundling 減少 HTTP 請求數量

5. **記憶體管理與物件重用**：
   - 使用 `ObjectPool<T>` 重用昂貴物件
   - 使用 `ArrayPool<T>` 減少陣列配置
   - 正確實作 `IDisposable` 並使用 `using` 陳述式
   - 避免裝箱（boxing）和不必要的配置
   - 使用 `Span<T>` 和 `Memory<T>` 處理切片操作

## 品質關卡

### 提交前檢查（開發人員本機）

- **編譯檢查**：
  - `dotnet build` 成功且零警告（啟用 `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`）
  - Roslyn 分析器無新問題（Code Analysis）
  - 無 Nullable 參考型別警告
  
- **測試執行**：
  - `dotnet test` 所有測試通過
  - 新增的測試已先失敗（驗證 TDD 流程）
  - 相關的整合測試已執行並通過
  
- **程式碼格式化**：
  - 程式碼已根據 `.editorconfig` 自動格式化
  - 使用 `dotnet format` 或 IDE 內建格式化工具
  - 無多餘的 using 陳述式（IDE 會自動整理）

### Pull Request 檢查（CI/CD）

**建構管線必須執行以下檢查**：

```yaml
# 範例檢查流程
1. 還原相依性: dotnet restore
2. 建構專案: dotnet build --no-restore --configuration Release
3. 執行單元測試: dotnet test --no-build --filter Category=Unit
4. 執行整合測試: dotnet test --no-build --filter Category=Integration
5. 程式碼覆蓋率: dotnet test --collect:"XPlat Code Coverage"
6. 靜態分析: dotnet build /p:RunAnalyzers=true /p:TreatWarningsAsErrors=true
7. 效能測試: dotnet run --project PerformanceTests
8. 安全掃描: dotnet list package --vulnerable --include-transitive
```

- **自動化測試**：
  - 所有單元測試通過（使用 `[Trait("Category", "Unit")]` 標記）
  - 所有整合測試通過（使用 `[Trait("Category", "Integration")]` 標記）
  - UI 測試通過（如適用，使用 `[Trait("Category", "UI")]` 標記）
  - 測試執行時間合理（單元測試 <5 分鐘，整合測試 <15 分鐘）
  
- **程式碼覆蓋率**：
  - 整體覆蓋率達到 80% 門檻
  - 新增程式碼的覆蓋率不得低於既有平均值
  - 使用 Coverlet 或 dotnet-coverage 產生報告
  - 將覆蓋率報告發佈到 PR 註解或儀表板（如 Codecov、SonarQube）
  
- **靜態分析**：
  - Roslyn 分析器無新問題（與基準分支比較）
  - Code Quality 分數未下降（如使用 SonarQube）
  - 無已知的高嚴重性問題（Medium 以上需要解釋或修復）
  
- **效能測試**：
  - 關鍵路徑的效能基準測試通過
  - 無效能回歸 >10%（與基準分支比較）
  - 記憶體使用未顯著增加（<10% 增長）
  
- **安全掃描**：
  - NuGet 套件無已知的高/嚴重漏洞（使用 `dotnet list package --vulnerable`）
  - 無硬編碼的敏感資訊（credentials、API keys、連線字串）
  - 使用 GitHub Dependabot 或 Snyk 自動化相依性掃描

### 正式環境前檢查

- **人工 QA 測試**：
  - 完成測試計畫中所有測試案例
  - 驗收測試通過（與 spec.md 中的驗收準則比對）
  - 探索性測試完成（測試未預期的使用者行為）
  - 跨瀏覽器測試（Chrome、Firefox、Safari、Edge）
  - 跨裝置測試（桌面、平板、手機）
  
- **無障礙稽核**：
  - 使用 axe DevTools 或 Lighthouse 執行自動化無障礙檢查
  - WCAG 2.1 Level AA 合規性驗證
  - 鍵盤導覽測試（所有功能可不使用滑鼠操作）
  - 螢幕閱讀器測試（NVDA 或 JAWS）
  
- **負載測試**：
  - 在預備環境執行負載測試（模擬預期流量的 150%）
  - 驗證無記憶體洩漏（長時間執行記憶體使用穩定）
  - 驗證自動擴展設定（如使用 Azure App Service 或 Kubernetes）
  - 壓力測試：找出系統崩潰點和瓶頸
  
- **資料格式版本管理**：
  - 在預備環境測試資料格式升級（JSON 結構變更）
  - 實作資料格式版本號機制（在 JSON 中加入 `version` 欄位）
  - 驗證向前相容性（舊版本資料能正確讀取）
  - 準備資料備份和還原程序（檔案複製和還原）
  - 測試資料完整性（格式變更前後資料一致）
  
- **部署就緒檢查**：
  - 記錄完整的回滾計畫（包含資料檔案備份和還原步驟）
  - 準備發布說明（新功能、破壞性變更、已知問題）
  - 驗證環境設定（環境變數、JSON 檔案路徑、功能旗標）
  - 確認資料檔案存取權限正確設定
  - 準備資料備份機制（自動備份 JSON 檔案）

## 治理規範

### 修訂流程

本憲章是活文件，可透過以下方式修訂：

1. **提案**：任何團隊成員可透過 pull request 提出變更
2. **討論**：最少 48 小時審查期供團隊回饋
3. **核准**：需要核心團隊多數核准
4. **遷移**：修訂包含現有程式碼的遷移計畫（如適用）
5. **文件化**：更新所有相依範本和文件

### 版本控制政策

憲章遵循語意化版本控制：

- **MAJOR（主版本）**：對原則的重大變更或移除原則
- **MINOR（次版本）**：新增原則或重大擴充指引
- **PATCH（修訂版本）**：錯字修正、格式改進、次要說明

### 合規與執行

- 所有 pull request 必須驗證是否符合憲章原則
- 程式碼審查必須明確檢查憲章遵循情況
- 不符合規範的程式碼無論多緊急都不得合併
- 違規需要回顧和流程改進
- 憲章優先於所有其他開發實務或口頭協議

### 活文件

本憲章是活文件，與專案程式碼同步演進。詳細工作流程和範本請參考 `.specify/templates/`：

- `plan-template.md`：與憲章原則對齊的實作規劃範本
- `spec-template.md`：功能規範格式範本
- `tasks-template.md`：任務分解結構範本
- `checklist-template.md`：檢查清單範本
- 所有範本會持續更新以保持與本憲章一致

**相關資源**：

- [ASP.NET Core 最佳實務](https://learn.microsoft.com/aspnet/core/fundamentals/best-practices)
- [.NET 效能最佳化指南](https://learn.microsoft.com/dotnet/core/diagnostics/performance-optimizations)
- [Entity Framework Core 效能](https://learn.microsoft.com/ef/core/performance/)
- [xUnit 測試文件](https://xunit.net/)

---

**版本**：1.2.0  
**制定日期**：2025-10-18  
**最後修訂**：2025-10-18  
**下次審查日期**：2026-01-18（建議每季審查）
