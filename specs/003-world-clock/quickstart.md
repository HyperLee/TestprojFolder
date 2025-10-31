# Quick Start: 世界時鐘功能開發

**Feature**: 003-world-clock  
**Date**: 2025-11-01  
**Purpose**: 為開發人員提供快速開始開發世界時鐘功能的指南

## 概述

世界時鐘是一個 ASP.NET Core Razor Pages 功能，顯示 10 個主要城市的即時時間，支援時區切換和夏令時間自動處理。主要邏輯在客戶端 JavaScript 中實作。

## 前置需求

### 開發環境

- **.NET 8.0 SDK** 或更高版本
- **Visual Studio Code** 或 **Visual Studio 2022+**
- **Node.js** (選用，用於前端工具)
- **Git** (版本控制)

### 專案設定

專案已存在於 `BNICalculate` 目錄中，無需建立新專案。

```bash
# 確認 .NET 版本
dotnet --version  # 應該是 8.0.x 或更高

# 確認專案可以建構
cd BNICalculate
dotnet build

# 確認測試可以執行
cd ../BNICalculate.Tests
dotnet test
```

## 功能分支

```bash
# 已自動建立並切換到功能分支
git branch --show-current
# 輸出: 003-world-clock

# 查看分支狀態
git status
```

## 專案結構

```text
BNICalculate/
├── Pages/
│   ├── WorldClock.cshtml           ← 新增（Razor 頁面）
│   ├── WorldClock.cshtml.cs        ← 新增（PageModel）
│   └── Shared/_Layout.cshtml       ← 可能需要修改（新增導覽連結）
├── wwwroot/
│   ├── css/
│   │   └── worldclock.css          ← 新增（樣式）
│   └── js/
│       └── worldclock.js           ← 新增（主要邏輯）

BNICalculate.Tests/
└── Integration/Pages/
    └── WorldClockPageTests.cs      ← 新增（整合測試）
```

## 開發流程（TDD）

### 步驟 1: 撰寫失敗測試（Red）

**檔案**: `BNICalculate.Tests/Integration/Pages/WorldClockPageTests.cs`

```csharp
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BNICalculate.Tests.Integration.Pages
{
    public class WorldClockPageTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public WorldClockPageTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task WorldClock_ReturnsSuccessAndCorrectContentType()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/WorldClock");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("text/html; charset=utf-8", 
                response.Content.Headers.ContentType?.ToString());
        }

        [Fact]
        public async Task WorldClock_DisplaysTenCities()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/WorldClock");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("台北", content);
            Assert.Contains("東京", content);
            Assert.Contains("倫敦", content);
            Assert.Contains("紐約", content);
            Assert.Contains("洛杉磯", content);
            Assert.Contains("巴黎", content);
            Assert.Contains("柏林", content);
            Assert.Contains("莫斯科", content);
            Assert.Contains("新加坡", content);
            Assert.Contains("悉尼", content);
        }
    }
}
```

**執行測試**:

```bash
cd BNICalculate.Tests
dotnet test --filter "FullyQualifiedName~WorldClockPageTests"
```

測試應該失敗（因為頁面尚未建立）。

### 步驟 2: 實作最小功能（Green）

#### 2.1 建立 PageModel

**檔案**: `BNICalculate/Pages/WorldClock.cshtml.cs`

```csharp
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BNICalculate.Pages
{
    /// <summary>
    /// 世界時鐘頁面模型
    /// </summary>
    public class WorldClockModel : PageModel
    {
        /// <summary>
        /// 處理 GET 請求
        /// </summary>
        public void OnGet()
        {
            // 純客戶端邏輯，無需伺服器端處理
        }
    }
}
```

#### 2.2 建立 Razor 頁面

**檔案**: `BNICalculate/Pages/WorldClock.cshtml`

```cshtml
@page
@model BNICalculate.Pages.WorldClockModel
@{
    ViewData["Title"] = "世界時鐘";
}

<div class="container mt-4">
    <h1 class="text-center mb-4">@ViewData["Title"]</h1>

    <!-- 主要時間顯示 -->
    <div class="main-clock" id="main-clock" role="region" aria-labelledby="main-clock-label">
        <h2 id="main-clock-label" class="visually-hidden">主要時間顯示</h2>
        <div class="city-name" id="main-city-name" aria-label="城市名稱">台北</div>
        <time class="time-display main" id="main-time" aria-live="polite" aria-atomic="true">
            --:--:--
        </time>
        <div class="timezone" id="main-timezone" aria-label="時區">GMT+8</div>
        <div class="date-display" id="main-date" aria-label="日期">-------</div>
    </div>

    <!-- 城市時間網格 -->
    <div class="city-grid mt-4" id="city-grid">
        <!-- 動態生成城市卡片 -->
    </div>
</div>

@section Scripts {
    <link rel="stylesheet" href="~/css/worldclock.css" asp-append-version="true" />
    <script src="~/js/worldclock.js" asp-append-version="true"></script>
}
```

#### 2.3 建立樣式

**檔案**: `BNICalculate/wwwroot/css/worldclock.css`

```css
/* CSS Variables */
:root {
  --main-bg: #f8f9fa;
  --card-bg: #ffffff;
  --text-dark: #212529;
  --text-gray: #495057;
  --text-light: #6c757d;
  --hover-bg: #e9ecef;
  --active-border: #007bff;
}

/* 主要時間區域 */
.main-clock {
  background: var(--card-bg);
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  border-radius: 12px;
  padding: 2rem;
  text-align: center;
  transition: transform 0.3s ease;
  margin: 0 auto;
  max-width: 600px;
}

/* 城市網格 */
.city-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
}

/* 城市卡片 */
.city-card {
  background: var(--card-bg);
  border: 2px solid transparent;
  border-radius: 8px;
  padding: 1rem;
  cursor: pointer;
  transition: all 0.2s ease;
  text-align: center;
}

.city-card:hover {
  background: var(--hover-bg);
  transform: translateY(-2px);
}

.city-card:active,
.city-card:focus {
  border-color: var(--active-border);
  outline: none;
}

/* 字體 */
.city-name {
  font-size: 16px;
  font-weight: 500;
  color: var(--text-gray);
  margin-bottom: 0.5rem;
}

.time-display.main {
  font-size: 48px;
  font-weight: 700;
  font-family: 'Courier New', monospace;
  color: var(--text-dark);
  margin: 1rem 0;
}

.time-display.city {
  font-size: 24px;
  font-weight: 600;
  font-family: 'Courier New', monospace;
  color: var(--text-dark);
  margin: 0.5rem 0;
}

.timezone {
  font-size: 14px;
  color: var(--text-gray);
  margin-top: 0.5rem;
}

.date-display {
  font-size: 14px;
  font-weight: 400;
  color: var(--text-light);
  margin-top: 0.5rem;
}

/* 響應式設計 */
@media (max-width: 767px) {
  .city-grid {
    grid-template-columns: 1fr;
  }
  
  .main-clock .time-display {
    font-size: 36px;
  }
}

@media (min-width: 768px) and (max-width: 1199px) {
  .city-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (min-width: 1200px) {
  .city-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}

/* 無障礙輔助 */
.visually-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border-width: 0;
}
```

#### 2.4 建立 JavaScript 邏輯

**檔案**: `BNICalculate/wwwroot/js/worldclock.js`

```javascript
(function () {
    'use strict';

    // 城市配置
    const DEFAULT_CITIES = [
        { id: "taipei", name: "台北", timeZone: "Asia/Taipei", offsetLabel: "GMT+8", hasDST: false },
        { id: "tokyo", name: "東京", timeZone: "Asia/Tokyo", offsetLabel: "GMT+9", hasDST: false },
        { id: "london", name: "倫敦", timeZone: "Europe/London", offsetLabel: "GMT+0/GMT+1", hasDST: true },
        { id: "new-york", name: "紐約", timeZone: "America/New_York", offsetLabel: "GMT-5/GMT-4", hasDST: true },
        { id: "los-angeles", name: "洛杉磯", timeZone: "America/Los_Angeles", offsetLabel: "GMT-8/GMT-7", hasDST: true },
        { id: "paris", name: "巴黎", timeZone: "Europe/Paris", offsetLabel: "GMT+1/GMT+2", hasDST: true },
        { id: "berlin", name: "柏林", timeZone: "Europe/Berlin", offsetLabel: "GMT+1/GMT+2", hasDST: true },
        { id: "moscow", name: "莫斯科", timeZone: "Europe/Moscow", offsetLabel: "GMT+3", hasDST: false },
        { id: "singapore", name: "新加坡", timeZone: "Asia/Singapore", offsetLabel: "GMT+8", hasDST: false },
        { id: "sydney", name: "悉尼", timeZone: "Australia/Sydney", offsetLabel: "GMT+10/GMT+11", hasDST: true }
    ];

    // 狀態管理
    let clockState = {
        mainCity: DEFAULT_CITIES[0],
        secondaryCities: DEFAULT_CITIES.slice(1),
        isRunning: false,
        timerId: null,
        lastUpdateTime: Date.now()
    };

    // Formatter 快取
    const formatterCache = new Map();

    // 取得時間 Formatter
    function getTimeFormatter(timeZone) {
        const key = `time-${timeZone}`;
        if (!formatterCache.has(key)) {
            formatterCache.set(key, new Intl.DateTimeFormat('zh-TW', {
                timeZone: timeZone,
                hour: '2-digit',
                minute: '2-digit',
                second: '2-digit',
                hour12: false
            }));
        }
        return formatterCache.get(key);
    }

    // 取得日期 Formatter
    function getDateFormatter(timeZone) {
        const key = `date-${timeZone}`;
        if (!formatterCache.has(key)) {
            formatterCache.set(key, new Intl.DateTimeFormat('zh-TW', {
                timeZone: timeZone,
                year: 'numeric',
                month: '2-digit',
                day: '2-digit'
            }));
        }
        return formatterCache.get(key);
    }

    // 更新所有時鐘
    function updateAllClocks() {
        const now = new Date();

        // 更新主要時間
        const mainTimeEl = document.getElementById('main-time');
        const mainDateEl = document.getElementById('main-date');
        if (mainTimeEl && mainDateEl) {
            mainTimeEl.textContent = getTimeFormatter(clockState.mainCity.timeZone).format(now);
            mainDateEl.textContent = getDateFormatter(clockState.mainCity.timeZone).format(now);
        }

        // 更新次要城市
        clockState.secondaryCities.forEach(city => {
            const timeEl = document.getElementById(`time-${city.id}`);
            if (timeEl) {
                timeEl.textContent = getTimeFormatter(city.timeZone).format(now);
            }
        });

        clockState.lastUpdateTime = Date.now();
    }

    // 啟動時鐘
    function startClock() {
        if (clockState.isRunning) return;

        updateAllClocks();
        clockState.timerId = setInterval(updateAllClocks, 1000);
        clockState.isRunning = true;
    }

    // 停止時鐘
    function stopClock() {
        if (clockState.timerId) {
            clearInterval(clockState.timerId);
            clockState.timerId = null;
        }
        clockState.isRunning = false;
    }

    // 初始化城市網格
    function initializeCityGrid() {
        const gridEl = document.getElementById('city-grid');
        if (!gridEl) return;

        clockState.secondaryCities.forEach(city => {
            const cardEl = document.createElement('button');
            cardEl.className = 'city-card';
            cardEl.id = `city-${city.id}`;
            cardEl.setAttribute('role', 'button');
            cardEl.setAttribute('aria-label', `切換到${city.name}時間`);
            cardEl.setAttribute('tabindex', '0');
            cardEl.innerHTML = `
                <div class="city-name">${city.name}</div>
                <time class="time-display city" id="time-${city.id}" aria-live="off">--:--:--</time>
                <div class="timezone">${city.offsetLabel}</div>
            `;
            gridEl.appendChild(cardEl);
        });
    }

    // 頁面載入時初始化
    document.addEventListener('DOMContentLoaded', function () {
        initializeCityGrid();
        startClock();
    });

    // 頁面卸載時清理
    window.addEventListener('beforeunload', stopClock);
    window.addEventListener('pagehide', stopClock);

    // 頁面可見性變化
    document.addEventListener('visibilitychange', function () {
        if (document.hidden) {
            stopClock();
        } else {
            startClock();
        }
    });
})();
```

**執行測試**:

```bash
cd BNICalculate.Tests
dotnet test --filter "FullyQualifiedName~WorldClockPageTests"
```

測試應該通過（Green）。

#### 2.5 新增導覽連結（可選）

**檔案**: `BNICalculate/Pages/Shared/_Layout.cshtml`

在導覽列中新增連結：

```html
<li class="nav-item">
    <a class="nav-link text-dark" asp-area="" asp-page="/WorldClock">世界時鐘</a>
</li>
```

### 步驟 3: 重構（Refactor）

執行程式碼格式化和品質檢查：

```bash
# 格式化程式碼
cd BNICalculate
dotnet format

# 建構檢查
dotnet build --no-restore --configuration Release

# 執行所有測試
cd ../BNICalculate.Tests
dotnet test
```

## 執行應用程式

```bash
cd BNICalculate
dotnet run

# 或使用 watch 模式（自動重載）
dotnet watch run
```

瀏覽器開啟 `https://localhost:5001/WorldClock` 或 `http://localhost:5000/WorldClock`

## 偵錯

### Visual Studio Code

1. 開啟 `WorldClock.cshtml.cs`
2. 設定中斷點
3. 按 `F5` 啟動偵錯
4. 瀏覽器導覽至 `/WorldClock`

### Visual Studio 2022

1. 開啟解決方案 `TestprojFolder.sln`
2. 設定中斷點
3. 按 `F5` 啟動偵錯

### 瀏覽器開發者工具

```javascript
// 在瀏覽器 Console 中檢查狀態
console.log(clockState);

// 手動更新時鐘
updateAllClocks();

// 停止時鐘
stopClock();
```

## 常見問題

### Q: 時間不更新？

**A**: 檢查瀏覽器 Console 是否有 JavaScript 錯誤。確認 `worldclock.js` 已正確載入。

### Q: 測試失敗？

**A**: 確認：

1. 專案已建構：`dotnet build`
2. `Program.cs` 中 `WebApplication` 可存取（`public partial class Program {}`）
3. 測試專案參考了主專案

### Q: 樣式沒有套用？

**A**: 確認：

1. `worldclock.css` 檔案路徑正確
2. `@section Scripts` 正確放置
3. 瀏覽器快取已清除（Ctrl+Shift+R）

## 下一步

完成基本功能後：

1. **新增城市切換功能** - 實作點選城市卡片切換主要顯示
2. **新增動畫效果** - 切換時的平滑過渡動畫
3. **無障礙測試** - 使用螢幕閱讀器和鍵盤導覽測試
4. **效能優化** - 使用 Chrome DevTools 測量效能
5. **跨瀏覽器測試** - 在 Firefox、Safari、Edge 測試

## 參考資源

- [專案文件](.)
- [功能規格](./spec.md)
- [技術研究](./research.md)
- [資料模型](./data-model.md)
- [合約定義](./contracts/)

---

**文件完成日期**: 2025-11-01  
**維護者**: 開發團隊
