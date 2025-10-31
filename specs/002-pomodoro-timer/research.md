# Research: 番茄工作法計時器技術研究

**Feature**: 番茄工作法計時器  
**Date**: 2025-10-31  
**Purpose**: 解決技術背景中的所有不確定性，為 Phase 1 設計提供技術決策依據

## 研究摘要

本文件記錄番茄工作法計時器功能的技術研究結果，涵蓋 JSON 檔案儲存、客戶端計時器實作、多視窗衝突偵測、圓形進度環視覺化等關鍵技術決策。所有研究基於 ASP.NET Core 8.0 Razor Pages 和 Vanilla JavaScript，遵循專案憲章的極簡原則。

---

## R1: JSON 檔案儲存最佳實務

### R1.1 決策

使用 **System.Text.Json** 進行 JSON 序列化/反序列化，搭配非同步檔案 I/O（File.ReadAllTextAsync / WriteAllTextAsync）。資料檔案儲存在 `wwwroot/data/` 目錄（可在執行時存取）或 `App_Data/`（建議用於非公開資料）。

### R1.2 理由

1. **效能**: System.Text.Json 是 .NET 內建高效能 JSON 函式庫，比 Newtonsoft.Json 快 2-5 倍
2. **零依賴**: 無需額外 NuGet 套件，符合憲章極簡原則
3. **非同步 I/O**: 避免阻塞主執行緒，提升應用程式回應性
4. **檔案位置考量**:
   - `wwwroot/data/`: 優點是簡單，缺點是檔案可被公開存取（需設定 web.config 限制）
   - `App_Data/`: ASP.NET 傳統做法，IIS 自動拒絕直接存取，更安全

### R1.3 實作模式

```csharp
// PomodoroDataService.cs 範例
public class PomodoroDataService
{
    private readonly string _dataPath;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public PomodoroDataService(IWebHostEnvironment env)
    {
        _dataPath = Path.Combine(env.ContentRootPath, "App_Data", "pomodoro");
        Directory.CreateDirectory(_dataPath); // 確保目錄存在
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true, // 開發時可讀性高
            PropertyNameCaseInsensitive = true
        };
    }
    
    public async Task<UserSettings> LoadSettingsAsync()
    {
        var filePath = Path.Combine(_dataPath, "settings.json");
        if (!File.Exists(filePath))
            return UserSettings.Default();
            
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<UserSettings>(json, _jsonOptions) 
               ?? UserSettings.Default();
    }
    
    public async Task SaveSettingsAsync(UserSettings settings)
    {
        var filePath = Path.Combine(_dataPath, "settings.json");
        var json = JsonSerializer.Serialize(settings, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }
}
```

### R1.4 考慮的替代方案

- **Newtonsoft.Json**: 功能豐富但效能較差且需額外依賴，不符合極簡原則
- **XML 序列化**: 檔案較大且不利於客戶端 JavaScript 讀取（若需要）
- **二進位序列化**: 不利於除錯和手動編輯，過度工程化

### R1.5 風險與緩解

- **併發寫入衝突**: 單一使用者場景下風險極低，若需要可使用 `FileStream` 加檔案鎖定
- **資料損壞**: 實作 try-catch 並在失敗時恢復預設值（已在規格 FR-023 中定義）

---

## R2: 客戶端計時器實作與時間準確性

### R2.1 決策

使用 **setInterval + Date.now() 校準機制**實作客戶端倒數計時器。每秒透過 `Date.now()` 重新計算剩餘時間，而非單純依賴 setInterval 的累積。

### R2.2 理由

1. **時間準確性**: setInterval 在瀏覽器背景執行、系統休眠或高負載時會產生延遲累積誤差
2. **校準機制**: 每次更新時基於「開始時間戳 + 總時長」計算剩餘時間，誤差不會累積
3. **符合規格**: 滿足 ±1 秒準確度要求（SC-002）
4. **簡單實作**: 不需要 Web Workers 或複雜的時間同步邏輯

### R2.3 實作模式

```javascript
// pomodoro.js 核心邏輯
class PomodoroTimer {
    constructor() {
        this.startTimestamp = null;
        this.totalDuration = 0; // 秒數
        this.intervalId = null;
    }
    
    start(durationMinutes) {
        this.totalDuration = durationMinutes * 60;
        this.startTimestamp = Date.now();
        
        this.intervalId = setInterval(() => {
            const elapsed = Math.floor((Date.now() - this.startTimestamp) / 1000);
            const remaining = Math.max(0, this.totalDuration - elapsed);
            
            this.updateDisplay(remaining);
            
            if (remaining === 0) {
                this.onComplete();
            }
        }, 1000);
    }
    
    pause() {
        // 儲存當前剩餘時間
        const elapsed = Math.floor((Date.now() - this.startTimestamp) / 1000);
        this.totalDuration = Math.max(0, this.totalDuration - elapsed);
        
        clearInterval(this.intervalId);
        this.intervalId = null;
        this.startTimestamp = null;
    }
    
    resume() {
        // 從暫停的剩餘時間繼續
        this.startTimestamp = Date.now();
        this.start(this.totalDuration / 60);
    }
}
```

### R2.4 考慮的替代方案

- **單純 setInterval**: 簡單但會累積誤差，不符合準確度要求
- **requestAnimationFrame**: 更精確（60fps）但過度工程化且耗電
- **Web Workers**: 可避免主執行緒阻塞但增加複雜度，小型專案不需要
- **伺服器端計時**: 需要 WebSocket/SignalR，違反「客戶端計時」設計原則

### R2.5 頁面離開/返回處理

規格 User Story 1 要求「使用者離開頁面再返回，計時器繼續正常倒數」。實作方式：

```javascript
// 頁面載入時檢查 localStorage
window.addEventListener('load', () => {
    const savedState = JSON.parse(localStorage.getItem('pomodoroState'));
    if (savedState && savedState.isRunning) {
        // 計算經過的時間
        const elapsed = Math.floor((Date.now() - savedState.startTimestamp) / 1000);
        const remaining = savedState.totalDuration - elapsed;
        
        if (remaining > 0) {
            timer.resume(remaining / 60);
        } else {
            timer.complete(); // 時間已到
        }
    }
});

// 頁面卸載時儲存狀態
window.addEventListener('beforeunload', () => {
    if (timer.isRunning) {
        localStorage.setItem('pomodoroState', JSON.stringify({
            isRunning: true,
            startTimestamp: timer.startTimestamp,
            totalDuration: timer.totalDuration,
            sessionType: timer.sessionType // 'work' or 'break'
        }));
    }
});
```

---

## R3: 多視窗衝突偵測機制

### R3.1 決策

使用 **localStorage + 心跳機制（heartbeat）+ storage 事件監聽**偵測多視窗開啟情況。第一個開啟的視窗取得「主視窗」身份，後續視窗偵測到主視窗存在時顯示警告並禁用功能。

### R3.2 理由

1. **即時偵測**: storage 事件可在視窗間即時通訊
2. **簡單可靠**: 無需 WebSocket 或伺服器端支援
3. **容錯性**: 心跳機制可偵測主視窗意外關閉，讓其他視窗接手

### R3.3 實作模式

```javascript
// 多視窗管理
class MultiWindowGuard {
    constructor() {
        this.windowId = Date.now() + Math.random(); // 唯一視窗 ID
        this.heartbeatInterval = null;
    }
    
    tryAcquireLock() {
        const lock = localStorage.getItem('pomodoroLock');
        
        if (!lock) {
            // 無主視窗，取得鎖定
            this.acquireLock();
            return true;
        }
        
        const lockData = JSON.parse(lock);
        const timeSinceLastHeartbeat = Date.now() - lockData.lastHeartbeat;
        
        if (timeSinceLastHeartbeat > 5000) {
            // 主視窗已失效（超過 5 秒無心跳），接管
            this.acquireLock();
            return true;
        }
        
        // 已有主視窗，顯示警告
        this.showMultiWindowWarning();
        return false;
    }
    
    acquireLock() {
        localStorage.setItem('pomodoroLock', JSON.stringify({
            windowId: this.windowId,
            lastHeartbeat: Date.now()
        }));
        
        // 每 2 秒發送心跳
        this.heartbeatInterval = setInterval(() => {
            const lock = JSON.parse(localStorage.getItem('pomodoroLock'));
            if (lock.windowId === this.windowId) {
                lock.lastHeartbeat = Date.now();
                localStorage.setItem('pomodoroLock', JSON.stringify(lock));
            }
        }, 2000);
    }
    
    releaseLock() {
        clearInterval(this.heartbeatInterval);
        localStorage.removeItem('pomodoroLock');
    }
    
    showMultiWindowWarning() {
        // 顯示橫幅警告並禁用控制按鈕
        document.getElementById('multi-window-warning').classList.remove('d-none');
        document.querySelectorAll('.timer-control').forEach(btn => {
            btn.disabled = true;
        });
    }
}

// 初始化
const windowGuard = new MultiWindowGuard();
if (!windowGuard.tryAcquireLock()) {
    // 偵測到多視窗，禁用功能
}

// 頁面關閉時釋放鎖定
window.addEventListener('beforeunload', () => {
    windowGuard.releaseLock();
});
```

### R3.4 考慮的替代方案

- **Broadcast Channel API**: 更現代但瀏覽器支援度較差（Safari 15.4+）
- **SharedWorker**: 複雜且除錯困難，過度工程化
- **伺服器端檢查**: 需要後端 API，違反「客戶端獨立運作」原則
- **不處理**: 接受資料衝突風險，不符合規格要求（Clarification #2）

---

## R4: 圓形進度環（Circular Progress Ring）實作

### R4.1 決策

使用 **SVG + CSS 動畫**實作圓形進度環，透過 `stroke-dasharray` 和 `stroke-dashoffset` 控制進度百分比。

### R4.2 理由

1. **效能優異**: 硬體加速的 CSS 動畫，不阻塞主執行緒
2. **零依賴**: 無需圖表函式庫（Chart.js, D3.js），符合極簡原則
3. **高度可控**: 可精確控制顏色、粗細、動畫速度
4. **可縮放**: SVG 向量圖形，任何解析度都清晰

### R4.3 實作模式

```html
<!-- Pomodoro.cshtml 中的 SVG -->
<svg class="progress-ring" width="200" height="200">
    <circle class="progress-ring__background"
            cx="100" cy="100" r="90"
            stroke="#e0e0e0" stroke-width="10" fill="none" />
    <circle class="progress-ring__circle"
            cx="100" cy="100" r="90"
            stroke="#4CAF50" stroke-width="10" fill="none"
            stroke-dasharray="565.48" stroke-dashoffset="0"
            transform="rotate(-90 100 100)" />
</svg>
<div class="timer-display">25:00</div>
```

```css
/* pomodoro.css */
.progress-ring__circle {
    transition: stroke-dashoffset 0.5s ease;
    stroke-linecap: round;
}

/* 工作時段顏色 */
.progress-ring__circle.work-phase {
    stroke: #4CAF50; /* 綠色 */
}

/* 休息時段顏色 */
.progress-ring__circle.break-phase {
    stroke: #2196F3; /* 藍色 */
}
```

```javascript
// pomodoro.js 進度更新
function updateProgress(remainingSeconds, totalSeconds) {
    const circle = document.querySelector('.progress-ring__circle');
    const radius = circle.r.baseVal.value;
    const circumference = 2 * Math.PI * radius; // 2πr = 565.48
    
    const progress = remainingSeconds / totalSeconds;
    const offset = circumference * (1 - progress);
    
    circle.style.strokeDashoffset = offset;
}
```

### R4.4 計算說明

- 圓周長 = 2πr = 2 × 3.14159 × 90 = 565.48
- stroke-dasharray: 設定為圓周長，定義虛線模式
- stroke-dashoffset: 從 0（0%）到 565.48（100%）控制顯示比例

### R4.5 考慮的替代方案

- **Canvas 2D API**: 需要手動繪製每幀，效能較差且程式碼複雜
- **CSS 漸變背景**: 難以實作圓形進度，僅適用於線性進度條
- **圖表函式庫（Chart.js）**: 功能過剩（40KB+），違反極簡原則
- **Lottie 動畫**: 需要設計師製作動畫檔案，過度依賴

---

## R5: 跨日界計時處理邏輯

### R5.1 決策

番茄鐘計數記錄在**工作時段開始的日期**（基於 Clarification #3）。系統在頁面載入時檢查「今日日期」，若與儲存的統計日期不符，則重置計數為 0。

### R5.2 理由

1. **符合使用者直覺**: 深夜工作的成果歸屬於當天
2. **簡化邏輯**: 無需處理時段分割或比例分配
3. **一致性**: 與大多數生產力工具（Toggl, RescueTime）的做法一致

### R5.3 實作模式

```csharp
// PomodoroStatistics.cs
public class PomodoroStatistics
{
    public string Date { get; set; } // "yyyy-MM-dd" 格式
    public int CompletedCount { get; set; }
    public List<TimerSession> Sessions { get; set; }
    
    public static PomodoroStatistics CreateForToday()
    {
        return new PomodoroStatistics
        {
            Date = DateTime.Today.ToString("yyyy-MM-dd"),
            CompletedCount = 0,
            Sessions = new List<TimerSession>()
        };
    }
    
    public bool IsToday()
    {
        return Date == DateTime.Today.ToString("yyyy-MM-dd");
    }
}
```

```csharp
// PomodoroDataService.cs
public async Task<PomodoroStatistics> LoadTodayStatsAsync()
{
    var filePath = Path.Combine(_dataPath, "stats.json");
    if (!File.Exists(filePath))
        return PomodoroStatistics.CreateForToday();
        
    var json = await File.ReadAllTextAsync(filePath);
    var stats = JsonSerializer.Deserialize<PomodoroStatistics>(json, _jsonOptions);
    
    if (stats == null || !stats.IsToday())
    {
        // 跨日了，重置統計
        return PomodoroStatistics.CreateForToday();
    }
    
    return stats;
}
```

```javascript
// 客戶端記錄完成時帶上開始時間戳
function recordPomodoroComplete(workStartTimestamp) {
    fetch('/Pomodoro/RecordComplete', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            WorkStartTimestamp: workStartTimestamp,
            WorkDuration: 25,
            BreakDuration: 5
        })
    });
}
```

### R5.4 邊界案例處理

- **23:50 開始工作，00:20 完成**: 計入前一天（工作開始日期）
- **午夜重新載入頁面**: 統計自動重置為 0
- **跨多日未使用**: 再次開啟時顯示今日計數 0（歷史資料可保留但不顯示）

---

## R6: 橫幅通知實作

### R6.1 決策

使用 **Bootstrap Toast 元件**或自訂 CSS 動畫實作 3-5 秒自動消失的橫幅通知（基於 Clarification #1）。

### R6.2 理由

1. **現有依賴**: 專案已使用 Bootstrap（見 wwwroot/lib/bootstrap），無需額外依賴
2. **可存取性**: Bootstrap Toast 支援 ARIA 屬性，符合憲章無障礙要求
3. **一致性**: 與專案其他頁面的通知樣式一致

### R6.3 實作模式（Bootstrap Toast）

```html
<!-- Pomodoro.cshtml -->
<div class="toast-container position-fixed top-0 start-50 translate-middle-x p-3">
    <div id="notification-toast" class="toast align-items-center" role="alert" aria-live="assertive" aria-atomic="true">
        <div class="d-flex">
            <div class="toast-body" id="toast-message">
                <!-- 動態訊息 -->
            </div>
            <button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast" aria-label="關閉"></button>
        </div>
    </div>
</div>
```

```javascript
// pomodoro.js 顯示通知
function showNotification(message, type = 'info') {
    const toastEl = document.getElementById('notification-toast');
    const toastBody = document.getElementById('toast-message');
    
    toastBody.textContent = message;
    toastEl.className = `toast align-items-center text-bg-${type}`;
    
    const toast = new bootstrap.Toast(toastEl, {
        autohide: true,
        delay: 4000 // 4 秒後自動關閉
    });
    
    toast.show();
}

// 使用範例
timer.onWorkComplete = () => {
    showNotification('工作時段完成！進入休息時段', 'success');
    startBreakSession();
};
```

### R6.4 訊息類型

- **工作完成**: 綠色 (success)，「工作時段完成！進入休息時段」
- **休息完成**: 藍色 (info)，「休息完成！番茄鐘 +1」
- **多視窗警告**: 橙色 (warning)，「偵測到多個視窗，已禁用計時功能」
- **資料恢復**: 黃色 (warning)，「偵測到資料異常，已恢復預設設定」
- **輸入錯誤**: 紅色 (danger)，「時長必須在 1-60 分鐘之間」

---

## R7: 輸入驗證與錯誤處理

### R7.1 決策

採用**前端 HTML5 驗證 + 後端 ModelState 驗證雙重防護**。前端使用 `<input type="number" min="1" max="60">` 提供即時回饋，後端使用 Data Annotations 確保資料安全。

### R7.2 理由

1. **使用者體驗**: 前端驗證提供即時回饋，無需提交表單
2. **安全性**: 後端驗證防止惡意請求繞過前端檢查
3. **標準做法**: 符合 ASP.NET Core 最佳實務

### R7.3 實作模式

```csharp
// UserSettings.cs
using System.ComponentModel.DataAnnotations;

public class UserSettings
{
    [Range(1, 60, ErrorMessage = "工作時長必須在 1-60 分鐘之間")]
    public int WorkDurationMinutes { get; set; } = 25;
    
    [Range(1, 30, ErrorMessage = "休息時長必須在 1-30 分鐘之間")]
    public int BreakDurationMinutes { get; set; } = 5;
}
```

```csharp
// Pomodoro.cshtml.cs
public class PomodoroModel : PageModel
{
    [BindProperty]
    public UserSettings Settings { get; set; }
    
    public async Task<IActionResult> OnPostSaveSettingsAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page(); // 返回頁面顯示錯誤訊息
        }
        
        await _dataService.SaveSettingsAsync(Settings);
        TempData["SuccessMessage"] = "設定已儲存";
        return RedirectToPage();
    }
}
```

```html
<!-- Pomodoro.cshtml 前端驗證 -->
<div class="mb-3">
    <label asp-for="Settings.WorkDurationMinutes" class="form-label">工作時長（分鐘）</label>
    <input asp-for="Settings.WorkDurationMinutes" 
           type="number" class="form-control" 
           min="1" max="60" required />
    <span asp-validation-for="Settings.WorkDurationMinutes" class="text-danger"></span>
</div>

<div class="mb-3">
    <label asp-for="Settings.BreakDurationMinutes" class="form-label">休息時長（分鐘）</label>
    <input asp-for="Settings.BreakDurationMinutes" 
           type="number" class="form-control" 
           min="1" max="30" required />
    <span asp-validation-for="Settings.BreakDurationMinutes" class="text-danger"></span>
</div>
```

---

## R8: 記憶體快取策略

### R8.1 決策

使用 **IMemoryCache** 快取使用者設定，避免每次頁面載入都讀取 JSON 檔案。統計資料不快取（需即時更新）。

### R8.2 理由

1. **效能提升**: 設定變更頻率低，快取可減少檔案 I/O
2. **內建支援**: IMemoryCache 是 ASP.NET Core 內建服務，無需額外依賴
3. **自動過期**: 支援滑動過期（SlidingExpiration），長時間無存取自動清除

### R8.3 實作模式

```csharp
// Program.cs 註冊服務
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<PomodoroDataService>();
```

```csharp
// PomodoroDataService.cs 使用快取
public class PomodoroDataService
{
    private readonly IMemoryCache _cache;
    private const string SETTINGS_CACHE_KEY = "PomodoroSettings";
    
    public PomodoroDataService(IWebHostEnvironment env, IMemoryCache cache)
    {
        _cache = cache;
        // ... 其他初始化
    }
    
    public async Task<UserSettings> LoadSettingsAsync()
    {
        // 嘗試從快取取得
        if (_cache.TryGetValue(SETTINGS_CACHE_KEY, out UserSettings cachedSettings))
        {
            return cachedSettings;
        }
        
        // 快取未命中，從檔案載入
        var settings = await LoadSettingsFromFileAsync();
        
        // 儲存到快取（10 分鐘滑動過期）
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10));
        _cache.Set(SETTINGS_CACHE_KEY, settings, cacheOptions);
        
        return settings;
    }
    
    public async Task SaveSettingsAsync(UserSettings settings)
    {
        await SaveSettingsToFileAsync(settings);
        
        // 更新快取
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10));
        _cache.Set(SETTINGS_CACHE_KEY, settings, cacheOptions);
    }
}
```

---

## 技術決策總結

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
| 跨日界處理 | 工作開始日期 | 符合使用者直覺 |

---

## 風險與緩解

| 風險 | 影響 | 緩解措施 |
|-----|------|---------|
| JSON 檔案併發寫入 | 中 | 單使用者場景風險低；若需要可加檔案鎖定 |
| 瀏覽器 localStorage 限制 | 低 | 儲存資料量極小（<10KB），遠低於 5-10MB 限制 |
| 計時器在背景不準確 | 中 | Date.now() 校準機制，誤差 <1 秒 |
| SVG 跨瀏覽器相容性 | 低 | 所有現代瀏覽器支援 SVG，目標平台已明確 |

---

## 下一步

✅ **Phase 0 完成** - 所有技術不確定性已解決

🔜 **Phase 1**: 基於本研究生成：

- data-model.md（實體類別設計）
- contracts/pomodoro-api.md（JavaScript API 規格）
- quickstart.md（開發者快速上手指南）
