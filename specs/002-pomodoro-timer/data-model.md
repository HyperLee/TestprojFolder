# Data Model: 番茄工作法計時器資料模型

**Feature**: 番茄工作法計時器  
**Date**: 2025-10-31  
**Purpose**: 定義所有資料實體的結構、屬性、驗證規則和關聯性

---

## 概述

本文件定義番茄工作法計時器功能的 4 個核心資料實體。所有實體使用 JSON 格式儲存於 `App_Data/pomodoro/` 目錄，透過 `System.Text.Json` 進行序列化/反序列化。

### 實體關係圖

```text
┌─────────────────┐
│  UserSettings   │  (1)  單一使用者設定
└─────────────────┘
        
┌─────────────────┐
│   TimerState    │  (1)  當前計時器狀態（僅存於 localStorage）
└─────────────────┘
        ↓ 完成後記錄
┌─────────────────┐
│  TimerSession   │  (N)  每個完成的工作/休息時段
└─────────────────┘
        ↓ 聚合為
┌─────────────────┐
│ PomodoroStats   │  (1 per day)  每日統計摘要
└─────────────────┘
```

### 檔案儲存結構

```text
App_Data/pomodoro/
├── settings.json         # UserSettings（單一檔案）
├── stats.json            # PomodoroStatistics（當日統計）
└── sessions/             # TimerSession（歷史記錄，可選）
    ├── 2025-10-31.json   # 每日時段明細
    └── 2025-10-30.json
```

**注意**: `TimerState` 不儲存於伺服器端，僅存於瀏覽器 localStorage 用於頁面恢復。

---

## E1: UserSettings（使用者設定）

### 用途

儲存使用者自訂的工作時長、休息時長等個人化設定。

### 屬性定義

```csharp
using System.ComponentModel.DataAnnotations;

namespace BNICalculate.Models;

/// <summary>
/// 番茄工作法使用者設定
/// </summary>
public class UserSettings
{
    /// <summary>
    /// 工作時段時長（分鐘）
    /// </summary>
    [Range(1, 60, ErrorMessage = "工作時長必須在 1-60 分鐘之間")]
    public int WorkDurationMinutes { get; set; } = 25;
    
    /// <summary>
    /// 休息時段時長（分鐘）
    /// </summary>
    [Range(1, 30, ErrorMessage = "休息時長必須在 1-30 分鐘之間")]
    public int BreakDurationMinutes { get; set; } = 5;
    
    /// <summary>
    /// 是否在時段完成時播放音效（未來擴充）
    /// </summary>
    public bool EnableSound { get; set; } = false;
    
    /// <summary>
    /// 最後修改時間（UTC）
    /// </summary>
    public DateTime LastModifiedUtc { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 建立預設設定
    /// </summary>
    public static UserSettings Default() => new UserSettings();
    
    /// <summary>
    /// 驗證設定有效性
    /// </summary>
    public bool IsValid()
    {
        return WorkDurationMinutes >= 1 && WorkDurationMinutes <= 60
            && BreakDurationMinutes >= 1 && BreakDurationMinutes <= 30;
    }
}
```

### JSON 範例

```json
{
  "workDurationMinutes": 25,
  "breakDurationMinutes": 5,
  "enableSound": false,
  "lastModifiedUtc": "2025-10-31T08:30:00Z"
}
```

### 驗證規則

| 屬性 | 規則 | 錯誤訊息 |
|-----|------|---------|
| WorkDurationMinutes | 1-60 | 工作時長必須在 1-60 分鐘之間 |
| BreakDurationMinutes | 1-30 | 休息時長必須在 1-30 分鐘之間 |
| LastModifiedUtc | 必填 | 自動設定 |

### 業務邏輯

- 預設值：25 分鐘工作 + 5 分鐘休息（經典番茄工作法）
- 儲存位置：`App_Data/pomodoro/settings.json`
- 快取策略：IMemoryCache 10 分鐘滑動過期
- 變更頻率：低（每次變更需手動儲存）

---

## E2: TimerSession（計時器時段記錄）

### E2 用途

記錄每次完成的工作時段或休息時段，用於統計和歷史追蹤。

### E2 屬性定義

```csharp
namespace BNICalculate.Models;

/// <summary>
/// 單次計時器時段記錄
/// </summary>
public class TimerSession
{
    /// <summary>
    /// 唯一識別碼（GUID）
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// 時段類型：work（工作）或 break（休息）
    /// </summary>
    public string SessionType { get; set; } = "work";
    
    /// <summary>
    /// 開始時間（UTC）
    /// </summary>
    public DateTime StartTimeUtc { get; set; }
    
    /// <summary>
    /// 結束時間（UTC）
    /// </summary>
    public DateTime EndTimeUtc { get; set; }
    
    /// <summary>
    /// 計畫時長（分鐘）
    /// </summary>
    public int PlannedDurationMinutes { get; set; }
    
    /// <summary>
    /// 實際時長（分鐘，計算屬性）
    /// </summary>
    public double ActualDurationMinutes => (EndTimeUtc - StartTimeUtc).TotalMinutes;
    
    /// <summary>
    /// 是否完整完成（未中途放棄）
    /// </summary>
    public bool IsCompleted { get; set; } = true;
    
    /// <summary>
    /// 記錄日期（本地時區，用於跨日界判斷）
    /// </summary>
    public string RecordDate { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");
    
    /// <summary>
    /// 建立工作時段
    /// </summary>
    public static TimerSession CreateWorkSession(int durationMinutes)
    {
        var now = DateTime.UtcNow;
        return new TimerSession
        {
            SessionType = "work",
            StartTimeUtc = now,
            EndTimeUtc = now.AddMinutes(durationMinutes),
            PlannedDurationMinutes = durationMinutes,
            IsCompleted = true
        };
    }
    
    /// <summary>
    /// 建立休息時段
    /// </summary>
    public static TimerSession CreateBreakSession(int durationMinutes)
    {
        var now = DateTime.UtcNow;
        return new TimerSession
        {
            SessionType = "break",
            StartTimeUtc = now,
            EndTimeUtc = now.AddMinutes(durationMinutes),
            PlannedDurationMinutes = durationMinutes,
            IsCompleted = true
        };
    }
}
```

### E2 JSON 範例

```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "sessionType": "work",
  "startTimeUtc": "2025-10-31T02:00:00Z",
  "endTimeUtc": "2025-10-31T02:25:00Z",
  "plannedDurationMinutes": 25,
  "actualDurationMinutes": 25.0,
  "isCompleted": true,
  "recordDate": "2025-10-31"
}
```

### E2 驗證規則

| 屬性 | 規則 | 說明 |
|-----|------|------|
| SessionType | "work" 或 "break" | 僅允許兩種類型 |
| StartTimeUtc | < EndTimeUtc | 結束時間必須晚於開始時間 |
| PlannedDurationMinutes | > 0 | 必須大於 0 |
| RecordDate | yyyy-MM-dd 格式 | 基於 StartTimeUtc 的本地日期 |

### E2 業務邏輯

- **跨日界處理**: RecordDate 基於 StartTimeUtc 的本地日期（例如：23:50 開始計入 10/31，而非 11/01）
- **實際時長計算**: 自動從 EndTimeUtc - StartTimeUtc 計算
- **儲存策略**: 每日記錄可合併至單一檔案（`sessions/2025-10-31.json`），或嵌入 PomodoroStatistics

---

## E3: PomodoroStatistics（番茄鐘統計）

### E3 用途

每日番茄鐘完成數量、工作總時長等統計摘要，顯示於 UI。

### E3 屬性定義

```csharp
namespace BNICalculate.Models;

/// <summary>
/// 番茄鐘每日統計資料
/// </summary>
public class PomodoroStatistics
{
    /// <summary>
    /// 統計日期（yyyy-MM-dd 格式）
    /// </summary>
    public string Date { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");
    
    /// <summary>
    /// 完成的番茄鐘數量（僅計算工作時段）
    /// </summary>
    public int CompletedPomodoroCount { get; set; } = 0;
    
    /// <summary>
    /// 完成的休息時段數量
    /// </summary>
    public int CompletedBreakCount { get; set; } = 0;
    
    /// <summary>
    /// 總工作時長（分鐘）
    /// </summary>
    public double TotalWorkMinutes { get; set; } = 0;
    
    /// <summary>
    /// 總休息時長（分鐘）
    /// </summary>
    public double TotalBreakMinutes { get; set; } = 0;
    
    /// <summary>
    /// 時段明細列表（可選，用於歷史追蹤）
    /// </summary>
    public List<TimerSession> Sessions { get; set; } = new List<TimerSession>();
    
    /// <summary>
    /// 最後更新時間（UTC）
    /// </summary>
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 建立今日統計
    /// </summary>
    public static PomodoroStatistics CreateForToday()
    {
        return new PomodoroStatistics
        {
            Date = DateTime.Today.ToString("yyyy-MM-dd"),
            CompletedPomodoroCount = 0,
            CompletedBreakCount = 0,
            TotalWorkMinutes = 0,
            TotalBreakMinutes = 0,
            Sessions = new List<TimerSession>(),
            LastUpdatedUtc = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// 檢查是否為今日統計
    /// </summary>
    public bool IsToday()
    {
        return Date == DateTime.Today.ToString("yyyy-MM-dd");
    }
    
    /// <summary>
    /// 記錄完成的工作時段
    /// </summary>
    public void RecordWorkSession(TimerSession session)
    {
        if (session.SessionType != "work") return;
        
        CompletedPomodoroCount++;
        TotalWorkMinutes += session.ActualDurationMinutes;
        Sessions.Add(session);
        LastUpdatedUtc = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 記錄完成的休息時段
    /// </summary>
    public void RecordBreakSession(TimerSession session)
    {
        if (session.SessionType != "break") return;
        
        CompletedBreakCount++;
        TotalBreakMinutes += session.ActualDurationMinutes;
        Sessions.Add(session);
        LastUpdatedUtc = DateTime.UtcNow;
    }
}
```

### E3 JSON 範例

```json
{
  "date": "2025-10-31",
  "completedPomodoroCount": 3,
  "completedBreakCount": 2,
  "totalWorkMinutes": 75.0,
  "totalBreakMinutes": 10.0,
  "sessions": [
    {
      "id": "uuid-1",
      "sessionType": "work",
      "startTimeUtc": "2025-10-31T01:00:00Z",
      "endTimeUtc": "2025-10-31T01:25:00Z",
      "plannedDurationMinutes": 25,
      "isCompleted": true,
      "recordDate": "2025-10-31"
    }
  ],
  "lastUpdatedUtc": "2025-10-31T09:30:00Z"
}
```

### E3 驗證規則

| 屬性 | 規則 | 說明 |
|-----|------|------|
| Date | yyyy-MM-dd 格式 | 必須為有效日期 |
| CompletedPomodoroCount | >= 0 | 不可為負數 |
| TotalWorkMinutes | >= 0 | 不可為負數 |
| Sessions | 可為空 | 可選保留歷史明細 |

### E3 業務邏輯

- **跨日重置**: 頁面載入時檢查 `IsToday()`，若非今日則建立新統計
- **即時更新**: 每次完成時段時立即更新計數和總時長
- **不快取**: 統計資料每次從檔案讀取，確保即時性
- **歷史保留**: Sessions 列表可選擇性清空以節省空間（保留 CompletedPomodoroCount 即可）

---

## E4: TimerState（計時器狀態）

### E4 用途

儲存計時器當前狀態於瀏覽器 localStorage，支援頁面離開/返回時恢復計時。

### E4 屬性定義

```typescript
// JavaScript/TypeScript 介面定義（非 C# 類別）
interface TimerState {
    /**
     * 計時器是否正在執行
     */
    isRunning: boolean;
    
    /**
     * 是否暫停中
     */
    isPaused: boolean;
    
    /**
     * 當前時段類型：'work' 或 'break'
     */
    sessionType: 'work' | 'break';
    
    /**
     * 開始時間戳（毫秒，Date.now()）
     */
    startTimestamp: number;
    
    /**
     * 總時長（秒數）
     */
    totalDuration: number;
    
    /**
     * 剩餘時間（秒數，暫停時記錄）
     */
    remainingSeconds: number;
    
    /**
     * 計畫時長（分鐘）
     */
    plannedDurationMinutes: number;
    
    /**
     * 最後更新時間戳（毫秒）
     */
    lastUpdateTimestamp: number;
}
```

### E4 JSON 範例（localStorage）

```json
{
  "isRunning": true,
  "isPaused": false,
  "sessionType": "work",
  "startTimestamp": 1730361600000,
  "totalDuration": 1500,
  "remainingSeconds": 1200,
  "plannedDurationMinutes": 25,
  "lastUpdateTimestamp": 1730361900000
}
```

### E4 驗證規則

| 屬性 | 規則 | 說明 |
|-----|------|------|
| sessionType | 'work' 或 'break' | 僅允許兩種類型 |
| totalDuration | > 0 | 必須大於 0 |
| remainingSeconds | >= 0 && <= totalDuration | 不可超過總時長 |
| startTimestamp | > 0 | 必須為有效時間戳 |

### E4 業務邏輯

- **儲存位置**: 瀏覽器 localStorage（key: `pomodoroState`）
- **更新頻率**: 每秒更新一次（setInterval）
- **頁面恢復**:
  1. 頁面載入時檢查 localStorage
  2. 若 `isRunning: true` 且 `lastUpdateTimestamp` < 5 分鐘前，計算經過時間
  3. 若剩餘時間 > 0，恢復計時；否則顯示完成通知
- **清除時機**:
  - 計時器完成時
  - 使用者點擊重置按鈕時
  - 偵測到多視窗衝突時
- **不同步至伺服器**: 純客戶端狀態，不持久化至 JSON 檔案

---

## 資料流程

### 工作流程範例

```text
1. 使用者點擊「開始工作」
   ↓
2. 建立 TimerState（isRunning: true, sessionType: 'work'）
   → 儲存至 localStorage
   ↓
3. 每秒更新 TimerState.remainingSeconds
   ↓
4. 25 分鐘後倒數結束
   ↓
5. 建立 TimerSession（work, 25 分鐘）
   ↓
6. 更新 PomodoroStatistics.CompletedPomodoroCount++
   → 儲存至 App_Data/pomodoro/stats.json
   ↓
7. 清除 TimerState，顯示「進入休息」通知
```

### 跨日界處理

```text
使用者 23:50 開始工作 25 分鐘
↓
00:15 完成（跨日）
↓
TimerSession.RecordDate = "2025-10-31"（工作開始日）
↓
PomodoroStatistics 仍為 10/31 的統計
↓
使用者重新載入頁面（00:20）
↓
偵測到 PomodoroStatistics.Date != Today
↓
建立新的 PomodoroStatistics（2025-11-01）
↓
顯示今日番茄鐘計數：0
```

---

## 序列化設定

### System.Text.Json 選項

```csharp
// PomodoroDataService.cs
private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,  // 忽略大小寫
    WriteIndented = true,                 // 格式化輸出（開發模式）
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // 忽略 null
    Converters = 
    { 
        new JsonStringEnumConverter()    // Enum 序列化為字串
    }
};
```

### 屬性命名慣例

- **C# 類別**: PascalCase（`WorkDurationMinutes`）
- **JSON 檔案**: camelCase（`workDurationMinutes`）
- **轉換**: 自動透過 `PropertyNameCaseInsensitive = true` 處理

---

## 資料遷移策略

### 未來版本相容性

若未來擴充屬性（例如：新增 `EnableSound`），需保證向後相容：

```csharp
// 載入時提供預設值
public async Task<UserSettings> LoadSettingsAsync()
{
    var filePath = Path.Combine(_dataPath, "settings.json");
    if (!File.Exists(filePath))
        return UserSettings.Default();
        
    var json = await File.ReadAllTextAsync(filePath);
    var settings = JsonSerializer.Deserialize<UserSettings>(json, _jsonOptions);
    
    // 若舊版本無 EnableSound，自動補齊預設值
    return settings ?? UserSettings.Default();
}
```

### 資料備份

- 每日統計檔案自動按日期分檔（`stats-2025-10-31.json`）
- 設定檔案修改前可選建立備份（`settings.json.bak`）

---

## 效能考量

| 實體 | 檔案大小估算 | 載入頻率 | 快取策略 |
|-----|------------|---------|---------|
| UserSettings | ~200 bytes | 每次頁面載入 | IMemoryCache 10 分鐘 |
| PomodoroStatistics | ~2KB（含 50 時段） | 每次完成時段 | 不快取（即時讀寫） |
| TimerSession | ~150 bytes/筆 | 僅歷史查詢 | 不快取 |
| TimerState | ~300 bytes | 每秒更新 | localStorage（客戶端） |

**總儲存空間**: 每日約 2-5 KB，年度約 1-2 MB（可接受）。

---

## 測試建議

### 單元測試

```csharp
[Fact]
public void UserSettings_Validation_ShouldRejectInvalidValues()
{
    var settings = new UserSettings { WorkDurationMinutes = 100 };
    Assert.False(settings.IsValid());
}

[Fact]
public void PomodoroStatistics_IsToday_ShouldReturnTrueForCurrentDate()
{
    var stats = PomodoroStatistics.CreateForToday();
    Assert.True(stats.IsToday());
}

[Fact]
public void TimerSession_ActualDuration_ShouldCalculateCorrectly()
{
    var session = new TimerSession
    {
        StartTimeUtc = new DateTime(2025, 10, 31, 1, 0, 0, DateTimeKind.Utc),
        EndTimeUtc = new DateTime(2025, 10, 31, 1, 25, 0, DateTimeKind.Utc)
    };
    Assert.Equal(25.0, session.ActualDurationMinutes);
}
```

### 整合測試

- 測試 JSON 序列化/反序列化往返
- 測試跨日界統計重置邏輯
- 測試檔案不存在時預設值載入

---

## 附錄：資料字典

| 術語 | 英文 | 說明 |
|-----|------|------|
| 番茄鐘 | Pomodoro | 一個完整的工作時段（預設 25 分鐘） |
| 時段 | Session | 工作或休息的單次計時週期 |
| 統計 | Statistics | 每日完成數量、總時長等彙總資料 |
| 狀態 | State | 計時器當前執行狀態（執行中/暫停/停止） |
| 設定 | Settings | 使用者自訂的時長和偏好 |
