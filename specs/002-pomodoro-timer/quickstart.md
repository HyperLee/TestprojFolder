# 快速開始指南：番茄工作法計時器

**Feature**: 番茄工作法計時器  
**Date**: 2025-10-31  
**Purpose**: 協助開發者快速設定本機環境、執行測試、開始開發

---

## 概述

本指南涵蓋番茄工作法計時器功能的本機開發環境設定、測試執行、偵錯技巧和常見問題排解。

### 先決條件

- ✅ .NET 8.0 SDK（執行 `dotnet --version` 確認）
- ✅ Visual Studio Code 或 Visual Studio 2022
- ✅ Git（用於版本控制）
- ✅ 現代瀏覽器（Chrome/Edge/Firefox）

---

## 步驟 1：取得程式碼

### Clone 專案（若為新開發者）

```bash
git clone https://github.com/HyperLee/TestprojFolder.git
cd TestprojFolder
```

### 切換至功能分支

```bash
git checkout 002-pomodoro-timer
```

---

## 步驟 2：環境設定

### 2.1 安裝相依套件

```bash
# 還原 NuGet 套件
dotnet restore

# 確認專案可建置
dotnet build
```

### 2.2 建立資料目錄

```bash
# 建立 JSON 儲存目錄
mkdir -p BNICalculate/App_Data/pomodoro
```

### 2.3 確認設定檔

檢查 `BNICalculate/appsettings.Development.json`：

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**注意**: 無需額外設定，JSON 檔案儲存路徑在程式碼中硬編碼為 `App_Data/pomodoro/`。

---

## 步驟 3：執行應用程式

### 3.1 啟動開發伺服器

```bash
cd BNICalculate
dotnet run
```

**預期輸出**:

```text
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### 3.2 開啟瀏覽器

前往 `https://localhost:5001/Pomodoro`

**預期畫面**:

- ✅ 顯示圓形進度環（25:00）
- ✅ 顯示控制按鈕（開始工作、設定）
- ✅ 顯示今日番茄鐘計數：0

---

## 步驟 4：執行測試

### 4.1 執行所有測試

```bash
cd ../BNICalculate.Tests
dotnet test
```

**預期輸出**:

```text
Passed!  - Failed:     0, Passed:    15, Skipped:     0, Total:    15, Duration: 2.3s
```

### 4.2 執行特定測試類別

```bash
# 僅執行資料服務測試
dotnet test --filter "FullyQualifiedName~PomodoroDataServiceTests"

# 僅執行整合測試
dotnet test --filter "Category=Integration"
```

### 4.3 產生測試覆蓋率報告

```bash
dotnet test --collect:"XPlat Code Coverage"
```

覆蓋率報告位於 `TestResults/<guid>/coverage.cobertura.xml`。

---

## 步驟 5：開發工作流程

### 5.1 專案結構導覽

```text
BNICalculate/
├── Pages/
│   ├── Pomodoro.cshtml          ← 主要視圖（HTML + Razor）
│   ├── Pomodoro.cshtml.cs       ← PageModel（C# 邏輯）
│   └── Shared/_Layout.cshtml    ← 共享版面配置
├── Models/
│   ├── TimerSession.cs          ← 實體類別
│   ├── PomodoroStatistics.cs    ← 統計類別
│   └── UserSettings.cs          ← 設定類別
├── Services/
│   └── PomodoroDataService.cs   ← JSON 檔案操作
├── wwwroot/
│   ├── js/pomodoro.js           ← 客戶端計時邏輯
│   └── css/pomodoro.css         ← 樣式
└── App_Data/pomodoro/           ← JSON 資料檔案
    ├── settings.json
    └── stats.json
```

### 5.2 常見開發任務

#### 修改計時器 UI

1. 編輯 `Pages/Pomodoro.cshtml`（HTML 結構）
2. 編輯 `wwwroot/css/pomodoro.css`（樣式）
3. 重新整理瀏覽器（熱重載自動生效）

#### 修改計時邏輯

1. 編輯 `wwwroot/js/pomodoro.js`
2. 重新整理瀏覽器（Ctrl+Shift+R 強制清除快取）
3. 開啟瀏覽器開發者工具（F12）檢查 Console

#### 修改伺服器端邏輯

1. 編輯 `Pages/Pomodoro.cshtml.cs` 或 `Services/PomodoroDataService.cs`
2. 儲存檔案（熱重載自動生效）
3. 若未生效，重啟 `dotnet run`

#### 新增測試

1. 在 `BNICalculate.Tests/Unit/Services/` 建立測試檔案
2. 遵循命名慣例：`<ClassName>Tests.cs`
3. 執行 `dotnet test` 確認測試通過

---

## 步驟 6：偵錯技巧

### 6.1 偵錯 C# 程式碼（VS Code）

1. 設定中斷點：點擊行號左側
2. 按 `F5` 啟動偵錯模式
3. 瀏覽至 `/Pomodoro` 觸發中斷點
4. 使用偵錯控制台檢查變數

**.vscode/launch.json** 範例：

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (web)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/BNICalculate/bin/Debug/net8.0/BNICalculate.dll",
      "args": [],
      "cwd": "${workspaceFolder}/BNICalculate",
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      }
    }
  ]
}
```

### 6.2 偵錯 JavaScript 程式碼

1. 開啟瀏覽器開發者工具（F12）
2. 切換至 `Sources` 標籤
3. 找到 `pomodoro.js` 並設定中斷點
4. 點擊「開始工作」按鈕觸發中斷點
5. 使用 `Console` 檢查變數：

```javascript
console.log('Timer state:', timer.state);
console.log('Remaining seconds:', timer.remainingSeconds);
```

### 6.3 檢查 localStorage

在瀏覽器 Console 執行：

```javascript
// 檢視計時器狀態
console.log(JSON.parse(localStorage.getItem('pomodoroState')));

// 檢視多視窗鎖定
console.log(JSON.parse(localStorage.getItem('pomodoroLock')));

// 清除狀態（用於測試）
localStorage.clear();
```

### 6.4 檢查 JSON 檔案

```bash
# 檢視設定
cat BNICalculate/App_Data/pomodoro/settings.json

# 檢視統計
cat BNICalculate/App_Data/pomodoro/stats.json

# 監控檔案變更（macOS/Linux）
watch -n 1 cat BNICalculate/App_Data/pomodoro/stats.json
```

---

## 步驟 7：常見問題排解

### Q1: 頁面載入出現 404 Not Found

**原因**: Razor Page 路由未正確設定。

**解決方案**:

1. 確認 `Pages/Pomodoro.cshtml` 存在
2. 確認檔案頂部有 `@page` 指令
3. 重新啟動應用程式

---

### Q2: 計時器狀態未恢復

**原因**: localStorage 未正確儲存或讀取。

**解決方案**:

1. 開啟瀏覽器 Console
2. 檢查錯誤訊息：`localStorage is not defined`（可能為隱私模式）
3. 確認 `pomodoro.js` 已載入：查看 Network 標籤
4. 清除 localStorage 重試：`localStorage.clear()`

---

### Q3: JSON 檔案無法寫入

**原因**: 權限問題或目錄不存在。

**解決方案**:

```bash
# 建立目錄
mkdir -p BNICalculate/App_Data/pomodoro

# 修改權限（若需要）
chmod 755 BNICalculate/App_Data/pomodoro

# 檢查程式碼中的路徑設定
grep -r "App_Data" BNICalculate/Services/
```

---

### Q4: 測試失敗：「找不到 JSON 檔案」

**原因**: 測試環境未建立測試資料目錄。

**解決方案**:

在測試的 `Setup` 方法中建立臨時目錄：

```csharp
[Fact]
public async Task LoadSettings_ShouldReturnDefault_WhenFileNotExists()
{
    // Arrange
    var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    Directory.CreateDirectory(tempDir);
    
    var env = new Mock<IWebHostEnvironment>();
    env.Setup(e => e.ContentRootPath).Returns(tempDir);
    
    var service = new PomodoroDataService(env.Object, new MemoryCache(new MemoryCacheOptions()));
    
    // Act
    var settings = await service.LoadSettingsAsync();
    
    // Assert
    Assert.NotNull(settings);
    Assert.Equal(25, settings.WorkDurationMinutes);
    
    // Cleanup
    Directory.Delete(tempDir, true);
}
```

---

### Q5: 多視窗警告一直顯示

**原因**: 心跳機制未正確清除。

**解決方案**:

1. 關閉所有番茄鐘頁面
2. 清除 localStorage：

   ```javascript
   localStorage.removeItem('pomodoroLock');
   ```

3. 重新開啟單一頁面

---

## 步驟 8：提交程式碼

### 8.1 確認變更

```bash
git status
git diff
```

### 8.2 執行完整檢查

```bash
# 建置
dotnet build

# 測試
dotnet test

# 程式碼格式化（若有安裝 dotnet-format）
dotnet format
```

### 8.3 提交並推送

```bash
git add .
git commit -m "feat: 新增番茄工作法計時器頁面"
git push origin 002-pomodoro-timer
```

### 8.4 建立 Pull Request

前往 GitHub 專案頁面建立 PR，確保：

- ✅ 所有測試通過
- ✅ 程式碼符合 `.editorconfig` 規範
- ✅ 已更新相關文件

---

## 步驟 9：進階主題

### 9.1 效能分析

使用 Chrome DevTools 分析效能：

1. 開啟 `Performance` 標籤
2. 點擊 `Record` 並操作計時器
3. 停止錄製並分析結果
4. 確認：
   - setInterval 執行時間 < 10ms
   - DOM 操作次數最小化
   - 記憶體無洩漏

### 9.2 測試覆蓋率視覺化

安裝 ReportGenerator：

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool

# 執行測試並產生覆蓋率
dotnet test --collect:"XPlat Code Coverage"

# 產生 HTML 報告
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

# 開啟報告
open coveragereport/index.html  # macOS
start coveragereport/index.html # Windows
```

### 9.3 自動化測試（CI/CD）

`.github/workflows/dotnet.yml` 範例：

```yaml
name: .NET

on:
  push:
    branches: [ 002-pomodoro-timer ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Test
      run: dotnet test --no-build --verbosity normal
```

---

## 資源連結

### 官方文件

- [ASP.NET Core Razor Pages](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/)
- [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-overview)
- [xUnit 測試框架](https://xunit.net/)

### 專案文件

- [Feature Specification](./spec.md) - 完整功能規格
- [Implementation Plan](./plan.md) - 實作計畫
- [Data Model](./data-model.md) - 資料模型設計
- [JavaScript API](./contracts/pomodoro-api.md) - 客戶端 API 規格

### 開發工具

- [Visual Studio Code](https://code.visualstudio.com/)
- [C# DevKit Extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
- [REST Client Extension](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) - 測試 API

---

## 取得協助

### 遇到問題？

1. **檢查 Console 錯誤**: 瀏覽器 F12 → Console
2. **檢查伺服器日誌**: 終端機視窗的 `dotnet run` 輸出
3. **查閱文件**: [spec.md](./spec.md), [data-model.md](./data-model.md)
4. **搜尋現有問題**: GitHub Issues
5. **建立新問題**: 提供錯誤訊息、環境資訊、重現步驟

### 開發社群

- GitHub Discussions: 提問與討論
- Code Review: 提交 PR 請求審查

---

## 附錄：鍵盤快捷鍵

### VS Code

| 快捷鍵 | 功能 |
|-------|------|
| `Ctrl+Shift+P` | 命令面板 |
| `F5` | 啟動偵錯 |
| `Ctrl+K Ctrl+C` | 註解程式碼 |
| `Ctrl+K Ctrl+U` | 取消註解 |
| `Ctrl+Shift+F` | 全域搜尋 |

### 瀏覽器

| 快捷鍵 | 功能 |
|-------|------|
| `F12` | 開啟開發者工具 |
| `Ctrl+Shift+R` | 強制重新整理（清除快取） |
| `Ctrl+Shift+C` | 元素選取器 |
| `Ctrl+Shift+J` | 開啟 Console |

---

## 下一步

✅ 環境已設定完成

🚀 開始開發：

1. 查閱 [tasks.md](./tasks.md)（由 `/speckit.tasks` 生成）
2. 選擇一個任務開始實作
3. 遵循 TDD 流程：先寫測試 → 實作 → 重構
4. 提交 PR 並請求審查

祝開發順利！🍅
