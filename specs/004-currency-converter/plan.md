# Implementation Plan: 台幣與外幣匯率計算器

**Branch**: `004-currency-converter` | **Date**: 2025年11月1日 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/004-currency-converter/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

建立一個台幣與外國貨幣匯率計算器，提供即時匯率查詢與計算功能。使用者可以輸入台幣或外幣金額，系統自動從台灣銀行官方 CSV API 取得最新匯率並進行雙向計算。支援七種主要外幣（USD、JPY、CNY、EUR、GBP、HKD、AUD），提供響應式介面，並透過 JSON 檔案快取匯率資料以提升效能。

技術方法：擴充現有 ASP.NET Core Razor Pages 專案（BNICalculate），新增匯率計算頁面，使用 HttpClient 呼叫台銀 CSV API，透過 System.Text.Json 解析並儲存至 JSON 檔案，使用 IMemoryCache 實作 30 分鐘快取機制。

## Technical Context

**Language/Version**: C# 12 / .NET 8.0  
**Primary Dependencies**: ASP.NET Core 8.0 Razor Pages（現有專案），System.Text.Json, IMemoryCache, Serilog  
**Storage**: JSON 檔案儲存（App_Data/currency/rates.json），無資料庫  
**Testing**: xUnit + WebApplicationFactory（整合測試），Moq（模擬外部 API）  
**Target Platform**: Web Application（跨瀏覽器：Chrome、Firefox、Safari、Edge）  
**Project Type**: Web Application（現有 BNICalculate 專案擴充，非新專案）  
**Performance Goals**: 頁面載入 <2 秒，計算回應 <3 秒，API 更新 <15 秒  
**Constraints**: 無資料庫使用，JSON 檔案儲存，台銀 API 15 秒逾時，30 分鐘快取，系統總負載 1000 次/小時  
**Scale/Scope**: 單一頁面功能擴充，7 種貨幣支援，預期同時使用者 <50 人

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Initial Check (Before Phase 0)

#### I. 程式碼品質標準

- ✅ 啟用 Nullable 參考型別（專案已設定）
- ✅ 使用 .editorconfig 定義程式碼風格
- ✅ 公開 API 使用 XML 文件註解（繁體中文）
- ✅ 遵循 SOLID 原則，使用相依性注入（IHttpClientFactory, ICurrencyService）
- ✅ Async 方法命名包含 `Async` 後綴

#### II. 測試標準

- ✅ 採用 TDD 工作流程：測試先行
- ✅ 使用 xUnit 框架
- ✅ 整合測試使用 WebApplicationFactory
- ✅ 外部 API 使用 Moq 模擬
- ✅ 目標覆蓋率：服務層 >80%，關鍵邏輯 100%
- ✅ 測試結構：Unit/, Integration/ 目錄分離

#### III. 使用者體驗一致性

- ✅ 響應式設計（行動優先）
- ✅ 使用現有 Bootstrap 5 樣式系統
- ✅ 錯誤訊息使用繁體中文，使用者友善
- ✅ 載入狀態顯示（>300ms 操作）
- ✅ 表單驗證即時回饋

#### IV. 效能要求

- ✅ 頁面載入 <2 秒（符合憲章要求）
- ✅ API 回應 <500 毫秒（符合憲章要求）
- ✅ 非同步檔案操作（File.ReadAllTextAsync/WriteAllTextAsync）
- ✅ IMemoryCache 快取機制（30 分鐘）
- ✅ System.Text.Json 高效能序列化

#### V. 文件與溝通語言

- ✅ 所有文件使用繁體中文
- ✅ UI 文字使用繁體中文
- ✅ XML 註解使用繁體中文

**初始評估結果**: ✅ 通過所有憲章要求，無違規項目

---

### Re-check (After Phase 1 Design)

**Re-check Date**: 2025年11月1日  
**Reviewed Artifacts**: research.md, data-model.md, contracts/taiwan-bank-api.md, contracts/currency-service-api.md, quickstart.md

#### I. 程式碼品質標準

- ✅ **data-model.md**: 所有實體定義包含完整 XML 文件註解（繁體中文）
- ✅ **contracts/**: 介面定義使用完整 XML `<summary>`, `<param>`, `<returns>`, `<exception>` 標籤
- ✅ **SOLID 原則**: 介面分離（ICurrencyService 與 ICurrencyDataService 分離），相依性注入（IHttpClientFactory, IMemoryCache, Serilog）
- ✅ **Async 命名**: 所有非同步方法包含 `Async` 後綴（FetchAndUpdateRatesAsync, GetRatesAsync, CalculateTwdToForeignAsync, LoadAsync, SaveAsync）
- ✅ **Nullable 參考型別**: 所有可為 null 的回傳型別明確標示（ExchangeRateData?, DateTime?）

#### II. 測試標準

- ✅ **quickstart.md**: 明確定義 TDD 流程（Red → Green → Refactor），包含 3 個測試範例（ExchangeRateTests, CurrencyDataServiceTests, CurrencyServiceTests）
- ✅ **測試結構**: 遵循 Unit/, Integration/ 目錄分離
- ✅ **測試範例**: 包含資料驗證測試、Theory 參數化測試、整合測試（WebApplicationFactory）
- ✅ **Moq 使用**: 文件中示範 `Mock<ICurrencyDataService>`、`Mock<IWebHostEnvironment>` 用法
- ✅ **覆蓋率目標**: 明確定義服務層 >80%，關鍵邏輯 100%

#### III. 使用者體驗一致性

- ✅ **Bootstrap 5**: quickstart.md 明確說明使用現有 Bootstrap 5 樣式
- ✅ **繁體中文錯誤訊息**: data-model.md 所有驗證屬性包含中文 ErrorMessage
- ✅ **載入狀態**: contracts/currency-service-api.md 的 PageModel 範例包含 `IsLoading` 屬性和前端載入動畫提示
- ✅ **表單驗證**: data-model.md 定義完整 Data Annotations（Required, Range, RegularExpression），quickstart.md 說明 jQuery Validation 整合

#### IV. 效能要求

- ✅ **快取策略**: contracts/currency-service-api.md 定義 30 分鐘 IMemoryCache 滑動過期
- ✅ **非同步 I/O**: data-model.md 和 contracts/ 所有檔案操作使用 async/await
- ✅ **逾時控制**: contracts/taiwan-bank-api.md 定義 HttpClient 15 秒逾時
- ✅ **效能優化**: quickstart.md 包含回應壓縮、靜態檔案快取建議
- ✅ **高效能序列化**: research.md 選擇 System.Text.Json（高效能原生支援）

#### V. 文件與溝通語言

- ✅ **所有文件**: research.md, data-model.md, contracts/, quickstart.md 全部使用繁體中文
- ✅ **XML 註解**: 所有程式碼範例的 XML 註解使用繁體中文
- ✅ **UI 文字**: PageModel 範例中的錯誤訊息、狀態文字使用繁體中文

**Phase 1 重新評估結果**: ✅ 通過所有憲章要求，無違規項目

**變更摘要**: Phase 1 設計文件完全符合憲章標準，所有程式碼範例、測試案例、文件註解均遵循憲章定義的程式碼品質、測試、UX、效能與文件要求。無需調整。

## Project Structure

### Documentation (this feature)

```text
specs/004-currency-converter/
├── plan.md              # 本檔案（技術規劃）
├── research.md          # Phase 0 輸出（技術研究）
├── data-model.md        # Phase 1 輸出（資料模型）
├── quickstart.md        # Phase 1 輸出（快速開始指南）
├── contracts/           # Phase 1 輸出（API 合約）
│   ├── taiwan-bank-api.md
│   └── currency-service-api.md
└── tasks.md             # Phase 2 輸出（由 /speckit.tasks 建立）
```

### Source Code (existing BNICalculate project)

```text
BNICalculate/
├── Pages/
│   ├── CurrencyConverter.cshtml          # 新增：匯率計算器頁面
│   ├── CurrencyConverter.cshtml.cs       # 新增：PageModel
│   └── Shared/
│       └── _Layout.cshtml                # 修改：新增導覽連結
├── Services/
│   ├── ICurrencyService.cs               # 新增：匯率服務介面
│   ├── CurrencyService.cs                # 新增：匯率服務實作
│   ├── ICurrencyDataService.cs           # 新增：資料存取介面
│   └── CurrencyDataService.cs            # 新增：JSON 檔案存取實作
├── Models/
│   ├── ExchangeRate.cs                   # 新增：匯率資料模型
│   ├── Currency.cs                       # 新增：貨幣模型
│   ├── CalculationRequest.cs             # 新增：計算請求模型
│   └── CalculationResult.cs              # 新增：計算結果模型
├── App_Data/
│   └── currency/
│       └── rates.json                    # 新增：匯率快取檔案
├── wwwroot/
│   ├── css/
│   │   └── currency-converter.css       # 新增：頁面樣式
│   └── js/
│       └── currency-converter.js         # 新增：前端互動邏輯
└── Program.cs                            # 修改：註冊服務

BNICalculate.Tests/
├── Unit/
│   └── Services/
│       ├── CurrencyServiceTests.cs       # 新增：服務單元測試
│       └── CurrencyDataServiceTests.cs   # 新增：資料存取單元測試
└── Integration/
    └── Pages/
        └── CurrencyConverterPageTests.cs # 新增：頁面整合測試
```

**Structure Decision**: 此功能擴充現有 ASP.NET Core Razor Pages 專案（BNICalculate），遵循現有架構模式（Pages/, Services/, Models/, wwwroot/）。新增匯率計算相關的頁面、服務、模型，並使用 App_Data/ 目錄儲存 JSON 快取檔案。測試專案維持現有結構（Unit/, Integration/），確保與現有測試一致性。

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
