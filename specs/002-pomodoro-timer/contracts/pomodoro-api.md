# JavaScript API 規格: 番茄工作法計時器

**文件版本**: 1.0  
**Date**: 2025-10-31  
**Purpose**: 定義客戶端 JavaScript API 的類別、方法、事件和使用範例

---

## 概述

番茄工作法計時器的核心邏輯在**客戶端執行**，使用 Vanilla JavaScript 實作。本文件定義 3 個主要類別和它們的公開 API。

### 架構圖

```text
┌─────────────────────────────────────────┐
│         Pomodoro.cshtml (UI)            │
│  ┌────────────┐  ┌─────────────────┐   │
│  │ 計時器顯示  │  │  控制按鈕區     │   │
│  │ (圓形進度)  │  │ (開始/暫停/重置) │   │
│  └────────────┘  └─────────────────┘   │
└─────────────────────────────────────────┘
              ↓ 呼叫 JavaScript API
┌─────────────────────────────────────────┐
│        pomodoro.js (邏輯層)             │
│  ┌──────────────────┐                   │
│  │  PomodoroTimer   │ ← 核心計時器邏輯  │
│  └──────────────────┘                   │
│  ┌──────────────────┐                   │
│  │ MultiWindowGuard │ ← 多視窗衝突偵測  │
│  └──────────────────┘                   │
│  ┌──────────────────┐                   │
│  │  NotificationUI  │ ← Toast 通知管理  │
│  └──────────────────┘                   │
└─────────────────────────────────────────┘
              ↓ 資料持久化
┌─────────────────────────────────────────┐
│      localStorage (客戶端儲存)          │
│  - pomodoroState (計時器狀態)           │
│  - pomodoroLock (多視窗鎖定)            │
└─────────────────────────────────────────┘
              ↓ 完成後記錄
┌─────────────────────────────────────────┐
│   伺服器端 API (POST /Pomodoro/...)     │
│  - RecordComplete (記錄統計)            │
└─────────────────────────────────────────┘
```

---

## API-1: PomodoroTimer 類別

### 用途

管理計時器核心邏輯：倒數、暫停、恢復、狀態追蹤。

### 建構函式

```javascript
/**
 * 建立番茄工作法計時器實例
 * @param {Object} options - 設定選項
 * @param {number} options.workDuration - 工作時長（分鐘），預設 25
 * @param {number} options.breakDuration - 休息時長（分鐘），預設 5
 * @param {Function} options.onTick - 每秒回呼函式 (remainingSeconds) => void
 * @param {Function} options.onWorkComplete - 工作完成回呼函式 () => void
 * @param {Function} options.onBreakComplete - 休息完成回呼函式 () => void
 * @param {Function} options.onStateChange - 狀態變更回呼函式 (state) => void
 */
constructor(options = {})
```

#### 範例

```javascript
const timer = new PomodoroTimer({
    workDuration: 25,
    breakDuration: 5,
    onTick: (remainingSeconds) => {
        updateTimerDisplay(remainingSeconds);
        updateProgressRing(remainingSeconds);
    },
    onWorkComplete: () => {
        showNotification('工作時段完成！進入休息時段', 'success');
        timer.startBreak();
    },
    onBreakComplete: () => {
        showNotification('休息完成！番茄鐘 +1', 'info');
        incrementPomodoroCount();
    }
});
```

### 屬性

| 屬性名稱 | 類型 | 描述 | 預設值 |
|---------|------|------|--------|
| `state` | `string` | 當前狀態：'idle', 'running', 'paused' | 'idle' |
| `sessionType` | `string` | 時段類型：'work', 'break' | 'work' |
| `startTimestamp` | `number` | 開始時間戳（毫秒） | null |
| `totalDuration` | `number` | 總時長（秒） | 1500 |
| `remainingSeconds` | `number` | 剩餘秒數 | 1500 |
| `intervalId` | `number` | setInterval ID | null |

### 方法

#### `startWork()`

開始工作時段。

```javascript
/**
 * 開始工作時段
 * @throws {Error} 如果計時器已在執行中
 */
startWork()
```

**範例**:

```javascript
document.getElementById('btn-start-work').addEventListener('click', () => {
    timer.startWork();
});
```

**副作用**:

- 設定 `state = 'running'`, `sessionType = 'work'`
- 記錄開始時間戳
- 啟動 setInterval（每秒觸發 `onTick`）
- 儲存狀態至 localStorage

---

#### `startBreak()`

開始休息時段。

```javascript
/**
 * 開始休息時段
 * @throws {Error} 如果計時器已在執行中
 */
startBreak()
```

**範例**:

```javascript
timer.onWorkComplete = () => {
    showNotification('工作完成！', 'success');
    timer.startBreak(); // 自動進入休息
};
```

---

#### `pause()`

暫停計時器。

```javascript
/**
 * 暫停當前計時器
 * @throws {Error} 如果計時器未執行
 */
pause()
```

**範例**:

```javascript
document.getElementById('btn-pause').addEventListener('click', () => {
    timer.pause();
    document.getElementById('btn-pause').style.display = 'none';
    document.getElementById('btn-resume').style.display = 'inline';
});
```

**副作用**:

- 計算並儲存剩餘時間
- 停止 setInterval
- 設定 `state = 'paused'`
- 更新 localStorage

---

#### `resume()`

從暫停狀態恢復計時。

```javascript
/**
 * 恢復計時器
 * @throws {Error} 如果計時器未暫停
 */
resume()
```

**範例**:

```javascript
document.getElementById('btn-resume').addEventListener('click', () => {
    timer.resume();
    document.getElementById('btn-resume').style.display = 'none';
    document.getElementById('btn-pause').style.display = 'inline';
});
```

---

#### `reset()`

重置計時器至初始狀態。

```javascript
/**
 * 重置計時器
 */
reset()
```

**範例**:

```javascript
document.getElementById('btn-reset').addEventListener('click', () => {
    if (confirm('確定要重置計時器嗎？')) {
        timer.reset();
    }
});
```

**副作用**:

- 停止計時（如有）
- 清除 localStorage 狀態
- 重置 UI 顯示
- 設定 `state = 'idle'`

---

#### `loadState()`

從 localStorage 載入狀態並恢復計時器。

```javascript
/**
 * 從 localStorage 載入狀態
 * @returns {boolean} 是否成功恢復狀態
 */
loadState()
```

**範例**:

```javascript
// 頁面載入時自動恢復
window.addEventListener('DOMContentLoaded', () => {
    if (timer.loadState()) {
        console.log('計時器狀態已恢復');
    } else {
        console.log('無先前狀態');
    }
});
```

**邏輯**:

1. 讀取 `localStorage.getItem('pomodoroState')`
2. 檢查 `lastUpdateTimestamp`（超過 5 分鐘視為無效）
3. 計算經過時間：`elapsed = Date.now() - startTimestamp`
4. 若 `remainingSeconds - elapsed > 0`，恢復計時
5. 若已逾時，觸發 `onComplete` 回呼

---

#### `saveState()`

儲存當前狀態至 localStorage。

```javascript
/**
 * 儲存狀態至 localStorage
 * @private
 */
saveState()
```

**內部呼叫**，不需手動執行。每次狀態變更時自動觸發。

---

### 事件回呼

| 回呼名稱 | 觸發時機 | 參數 |
|---------|---------|------|
| `onTick` | 每秒更新 | `remainingSeconds: number` |
| `onWorkComplete` | 工作時段結束 | 無 |
| `onBreakComplete` | 休息時段結束 | 無 |
| `onStateChange` | 狀態變更（idle/running/paused） | `state: string` |

---

### 使用範例：完整流程

```javascript
// 初始化計時器
const timer = new PomodoroTimer({
    workDuration: 25,
    breakDuration: 5,
    onTick: (remainingSeconds) => {
        const minutes = Math.floor(remainingSeconds / 60);
        const seconds = remainingSeconds % 60;
        document.getElementById('timer-display').textContent = 
            `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
        
        // 更新進度環
        const progress = remainingSeconds / timer.totalDuration;
        updateProgressRing(progress);
    },
    onWorkComplete: () => {
        showNotification('工作完成！開始休息', 'success');
        timer.startBreak();
    },
    onBreakComplete: async () => {
        showNotification('休息完成！番茄鐘 +1', 'info');
        
        // 記錄完成至伺服器
        await fetch('/Pomodoro/RecordComplete', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                sessionType: 'work',
                durationMinutes: 25,
                completedAtUtc: new Date().toISOString()
            })
        });
        
        // 更新計數顯示
        incrementPomodoroCount();
    },
    onStateChange: (state) => {
        console.log('Timer state changed:', state);
    }
});

// 頁面載入時恢復狀態
window.addEventListener('DOMContentLoaded', () => {
    timer.loadState();
});

// 按鈕事件綁定
document.getElementById('btn-start-work').addEventListener('click', () => timer.startWork());
document.getElementById('btn-pause').addEventListener('click', () => timer.pause());
document.getElementById('btn-resume').addEventListener('click', () => timer.resume());
document.getElementById('btn-reset').addEventListener('click', () => timer.reset());
```

---

## API-2: MultiWindowGuard 類別

### API-2 用途

偵測並防止多個瀏覽器視窗/分頁同時執行計時器，避免資料衝突。

### API-2 建構函式

```javascript
/**
 * 建立多視窗防護實例
 * @param {Object} options - 設定選項
 * @param {number} options.heartbeatInterval - 心跳間隔（毫秒），預設 2000
 * @param {number} options.lockTimeout - 鎖定超時（毫秒），預設 5000
 * @param {Function} options.onConflict - 衝突回呼函式 () => void
 */
constructor(options = {})
```

### API-2 屬性

| 屬性名稱 | 類型 | 描述 |
|---------|------|------|
| `windowId` | `string` | 當前視窗唯一 ID（時間戳 + 隨機數） |
| `isMainWindow` | `boolean` | 是否為主視窗 |
| `heartbeatIntervalId` | `number` | 心跳 setInterval ID |

### API-2 方法

#### `tryAcquireLock()`

嘗試取得主視窗鎖定。

```javascript
/**
 * 嘗試取得鎖定
 * @returns {boolean} 是否成功取得鎖定
 */
tryAcquireLock()
```

**範例**:

```javascript
const guard = new MultiWindowGuard({
    onConflict: () => {
        showNotification('偵測到多個視窗，已禁用計時功能', 'warning');
        disableAllControls();
    }
});

if (!guard.tryAcquireLock()) {
    console.warn('已有其他視窗執行計時器');
}
```

**邏輯**:

1. 檢查 `localStorage.getItem('pomodoroLock')`
2. 若無鎖定或鎖定已逾時（> 5 秒無心跳），取得鎖定
3. 若已有主視窗，觸發 `onConflict` 回呼

---

#### `releaseLock()`

釋放鎖定。

```javascript
/**
 * 釋放鎖定
 */
releaseLock()
```

**範例**:

```javascript
// 頁面關閉時釋放
window.addEventListener('beforeunload', () => {
    guard.releaseLock();
});
```

---

#### `startHeartbeat()`

啟動心跳機制（內部使用）。

```javascript
/**
 * 啟動心跳
 * @private
 */
startHeartbeat()
```

每 2 秒更新 `localStorage` 的 `lastHeartbeat` 時間戳。

---

### 使用範例

```javascript
// 初始化多視窗防護
const guard = new MultiWindowGuard({
    heartbeatInterval: 2000,
    lockTimeout: 5000,
    onConflict: () => {
        // 顯示警告橫幅
        document.getElementById('multi-window-warning').classList.remove('d-none');
        
        // 禁用所有控制按鈕
        document.querySelectorAll('.timer-control').forEach(btn => {
            btn.disabled = true;
        });
    }
});

// 嘗試取得鎖定
if (guard.tryAcquireLock()) {
    console.log('成功取得主視窗鎖定');
    initializeTimer();
} else {
    console.warn('偵測到多視窗衝突');
}

// 頁面關閉時清理
window.addEventListener('beforeunload', () => {
    guard.releaseLock();
});
```

---

## API-3: NotificationUI 工具函式

### API-3 用途

管理 Bootstrap Toast 通知顯示。

### API-3 函式定義

#### `showNotification(message, type)`

顯示 Toast 通知。

```javascript
/**
 * 顯示通知
 * @param {string} message - 通知訊息
 * @param {string} type - 通知類型：'success', 'info', 'warning', 'danger'
 * @param {number} duration - 顯示時長（毫秒），預設 4000
 */
function showNotification(message, type = 'info', duration = 4000)
```

**範例**:

```javascript
// 工作完成通知
showNotification('工作時段完成！進入休息時段', 'success');

// 錯誤通知
showNotification('時長必須在 1-60 分鐘之間', 'danger');

// 警告通知
showNotification('偵測到多個視窗', 'warning', 6000);
```

**實作**:

```javascript
function showNotification(message, type = 'info', duration = 4000) {
    const toastEl = document.getElementById('notification-toast');
    const toastBody = document.getElementById('toast-message');
    
    toastBody.textContent = message;
    toastEl.className = `toast align-items-center text-bg-${type}`;
    
    const toast = new bootstrap.Toast(toastEl, {
        autohide: true,
        delay: duration
    });
    
    toast.show();
}
```

---

## 輔助函式

### `updateProgressRing(progress)`

更新圓形進度環。

```javascript
/**
 * 更新進度環
 * @param {number} progress - 進度百分比 0-1
 */
function updateProgressRing(progress) {
    const circle = document.querySelector('.progress-ring__circle');
    const radius = circle.r.baseVal.value;
    const circumference = 2 * Math.PI * radius;
    
    const offset = circumference * (1 - progress);
    circle.style.strokeDashoffset = offset;
}
```

---

### `formatTime(seconds)`

格式化時間顯示。

```javascript
/**
 * 格式化時間為 MM:SS
 * @param {number} seconds - 秒數
 * @returns {string} 格式化字串（例如："25:00"）
 */
function formatTime(seconds) {
    const minutes = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
}
```

---

## 錯誤處理

### 常見錯誤場景

| 場景 | 錯誤處理 | 使用者回饋 |
|-----|---------|-----------|
| 重複開始計時器 | `throw new Error('Timer is already running')` | 按鈕 disabled |
| 暫停未執行的計時器 | `throw new Error('Timer is not running')` | 按鈕 disabled |
| localStorage 存取失敗 | `console.error`, 回退至記憶體狀態 | Toast 警告訊息 |
| 伺服器 API 失敗 | `console.error`, 重試 3 次 | Toast 錯誤訊息 |

### 錯誤處理範例

```javascript
try {
    timer.startWork();
} catch (error) {
    console.error('Failed to start timer:', error);
    showNotification('啟動計時器失敗，請重新整理頁面', 'danger');
}
```

---

## 效能考量

| 項目 | 規格 | 實作策略 |
|-----|------|---------|
| setInterval 頻率 | 1000ms | 避免過度更新 UI |
| localStorage 寫入 | 僅狀態變更時 | 減少 I/O 次數 |
| DOM 操作 | 最小化 | 使用 `textContent` 而非 `innerHTML` |
| 事件監聽器 | 一次性綁定 | 避免重複註冊 |

---

## 測試建議

### 單元測試（Jest/Mocha）

```javascript
describe('PomodoroTimer', () => {
    test('should start work session', () => {
        const timer = new PomodoroTimer();
        timer.startWork();
        expect(timer.state).toBe('running');
        expect(timer.sessionType).toBe('work');
    });
    
    test('should calculate remaining time correctly', () => {
        const timer = new PomodoroTimer();
        timer.startWork();
        // 模擬經過 5 秒
        jest.advanceTimersByTime(5000);
        expect(timer.remainingSeconds).toBe(1495);
    });
});
```

### 整合測試（Playwright）

```javascript
test('timer should countdown correctly', async ({ page }) => {
    await page.goto('/Pomodoro');
    await page.click('#btn-start-work');
    
    // 等待 2 秒
    await page.waitForTimeout(2000);
    
    const display = await page.textContent('#timer-display');
    expect(display).toMatch(/24:5[0-9]/); // 應顯示約 24:58
});
```

---

## 版本歷史

| 版本 | 日期 | 變更說明 |
|-----|------|---------|
| 1.0 | 2025-10-31 | 初始版本，定義 3 個核心 API |

---

## 附錄：完整 API 參考

### PomodoroTimer

- `constructor(options)`
- `startWork()`
- `startBreak()`
- `pause()`
- `resume()`
- `reset()`
- `loadState()`
- `saveState()` (private)

### MultiWindowGuard

- `constructor(options)`
- `tryAcquireLock()`
- `releaseLock()`
- `startHeartbeat()` (private)

### 工具函式

- `showNotification(message, type, duration)`
- `updateProgressRing(progress)`
- `formatTime(seconds)`
