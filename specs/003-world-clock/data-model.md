# Data Model: 世界時鐘

**Feature**: 003-world-clock  
**Date**: 2025-11-01  
**Purpose**: 定義世界時鐘功能的資料結構和狀態管理模型

## 概述

世界時鐘功能主要在客戶端運行，不需要伺服器端資料存儲。所有資料模型都是 JavaScript 物件，用於管理城市時區資訊和當前顯示狀態。

## 核心實體

### 1. CityTimezone (城市時區)

代表一個城市的時區資訊配置。

**屬性**:

| 屬性名稱 | 類型 | 必填 | 說明 | 範例值 |
|---------|------|------|------|--------|
| `id` | string | ✅ | 城市唯一識別符（用於 DOM ID） | `"taipei"`, `"tokyo"` |
| `name` | string | ✅ | 城市顯示名稱（繁體中文） | `"台北"`, `"東京"` |
| `timeZone` | string | ✅ | IANA 時區識別符 | `"Asia/Taipei"`, `"Asia/Tokyo"` |
| `offsetLabel` | string | ✅ | 時區偏移量標籤 | `"GMT+8"`, `"GMT+9"` |
| `hasDST` | boolean | ✅ | 是否支援夏令時間 | `false`, `true` |

**說明**:

- `timeZone` 使用 IANA Time Zone Database 標準識別符
- `offsetLabel` 為靜態標籤，實際偏移量由 `Intl.DateTimeFormat` 動態計算
- `hasDST` 用於文件說明，實際夏令時間由瀏覽器自動處理

**範例**:

```javascript
const taipeiCity = {
  id: "taipei",
  name: "台北",
  timeZone: "Asia/Taipei",
  offsetLabel: "GMT+8",
  hasDST: false
};

const newYorkCity = {
  id: "new-york",
  name: "紐約",
  timeZone: "America/New_York",
  offsetLabel: "GMT-5/GMT-4",
  hasDST: true
};
```

**驗證規則**:

- `id` 必須是唯一的、小寫、使用連字號分隔
- `name` 不可為空字串
- `timeZone` 必須是有效的 IANA 時區識別符
- `offsetLabel` 格式為 `GMT±N` 或 `GMT±N/GMT±M`（夏令時間）

### 2. ClockState (時鐘狀態)

代表當前時鐘的顯示狀態。

**屬性**:

| 屬性名稱 | 類型 | 必填 | 說明 | 範例值 |
|---------|------|------|------|--------|
| `mainCity` | CityTimezone | ✅ | 主要顯示的城市 | `taipeiCity` 物件 |
| `secondaryCities` | CityTimezone[] | ✅ | 次要顯示的城市列表（9 個） | `[tokyoCity, ...]` |
| `isRunning` | boolean | ✅ | 時鐘是否正在運行 | `true`, `false` |
| `timerId` | number \| null | ✅ | setInterval 計時器 ID | `123`, `null` |
| `lastUpdateTime` | number | ✅ | 最後更新的時間戳（毫秒） | `1698825600000` |

**說明**:

- `mainCity` 預設為台北，使用者可透過點選切換
- `secondaryCities` 始終包含 9 個城市（總共 10 個城市）
- `isRunning` 用於控制更新循環（頁面可見性變化時切換）
- `timerId` 用於清理計時器，防止記憶體洩漏
- `lastUpdateTime` 用於偵測長時間暫停後的時間跳躍

**範例**:

```javascript
const clockState = {
  mainCity: {
    id: "taipei",
    name: "台北",
    timeZone: "Asia/Taipei",
    offsetLabel: "GMT+8",
    hasDST: false
  },
  secondaryCities: [
    { id: "tokyo", name: "東京", timeZone: "Asia/Tokyo", offsetLabel: "GMT+9", hasDST: false },
    { id: "london", name: "倫敦", timeZone: "Europe/London", offsetLabel: "GMT+0/GMT+1", hasDST: true },
    // ... 其他 7 個城市
  ],
  isRunning: true,
  timerId: 12345,
  lastUpdateTime: 1698825600000
};
```

**狀態轉換**:

```text
初始化 → 運行中 ↔ 暫停 → 清理
         ↓
      切換城市（mainCity ↔ secondaryCities）
```

**驗證規則**:

- `mainCity` 不可為 null
- `secondaryCities` 必須包含正好 9 個城市
- `secondaryCities` 不可包含與 `mainCity` 相同的城市
- `isRunning` 為 true 時，`timerId` 不可為 null

### 3. FormattedTime (格式化時間)

代表某個時區在某個時刻的格式化時間資訊（運行時計算）。

**屬性**:

| 屬性名稱 | 類型 | 必填 | 說明 | 範例值 |
|---------|------|------|------|--------|
| `timeString` | string | ✅ | 格式化的時間字串（HH:mm:ss） | `"14:30:45"` |
| `dateString` | string | ✅ | 格式化的日期字串（YYYY-MM-DD） | `"2025-11-01"` |
| `offsetDisplay` | string | ✅ | 當前時區偏移量顯示 | `"GMT+8"` |
| `isDST` | boolean | ✅ | 當前是否為夏令時間 | `false`, `true` |

**說明**:

- 此結構不持久化，每次更新時由 `Intl.DateTimeFormat` 即時計算
- `timeString` 使用 24 小時制，符合規格要求
- `isDST` 由瀏覽器自動判斷（透過 `Intl.DateTimeFormat` 的 `timeZoneName` 選項）

**範例**:

```javascript
const formattedTime = {
  timeString: "14:30:45",
  dateString: "2025-11-01",
  offsetDisplay: "GMT+8",
  isDST: false
};
```

## 資料流程

### 初始化流程

```text
1. 載入頁面
   ↓
2. 初始化 cityConfigs（10 個城市配置）
   ↓
3. 設定 mainCity = 台北
   ↓
4. 設定 secondaryCities = 其他 9 個城市
   ↓
5. 建立 Formatter 實例（重用優化）
   ↓
6. 啟動時鐘（startClock）
   ↓
7. 開始定時更新（setInterval, 1000ms）
```

### 更新流程

```text
每秒觸發:
1. 取得當前時間（new Date()）
   ↓
2. 對每個城市：
   a. 使用 Formatter 計算時間字串
   b. 計算日期字串（主要城市）
   c. 更新 DOM 元素
   ↓
3. 更新 lastUpdateTime
```

### 切換城市流程

```text
使用者點選城市卡片:
1. 找到被點選的城市（event.target）
   ↓
2. 從 secondaryCities 中找到對應的 CityTimezone
   ↓
3. 交換 mainCity 和被點選城市：
   a. selectedCity → mainCity
   b. 原 mainCity → secondaryCities（替換被點選位置）
   ↓
4. 更新 DOM 結構
   ↓
5. 播放切換動畫
   ↓
6. 更新 ARIA 標籤（無障礙）
```

### 頁面可見性變化流程

```text
頁面隱藏（visibilitychange: hidden）:
1. 停止計時器（clearInterval）
   ↓
2. 設定 isRunning = false
   ↓
3. 記錄暫停時間

頁面顯示（visibilitychange: visible）:
1. 設定 isRunning = true
   ↓
2. 立即更新所有時間（補償延遲）
   ↓
3. 重新啟動計時器（setInterval）
```

### 清理流程

```text
頁面卸載（beforeunload / pagehide）:
1. 停止計時器（clearInterval）
   ↓
2. 設定 timerId = null
   ↓
3. 清除事件監聽器
   ↓
4. 釋放 Formatter 實例（GC 自動回收）
```

## 配置資料

### 預設城市配置（cityConfigs）

```javascript
const cityConfigs = [
  {
    id: "taipei",
    name: "台北",
    timeZone: "Asia/Taipei",
    offsetLabel: "GMT+8",
    hasDST: false
  },
  {
    id: "tokyo",
    name: "東京",
    timeZone: "Asia/Tokyo",
    offsetLabel: "GMT+9",
    hasDST: false
  },
  {
    id: "london",
    name: "倫敦",
    timeZone: "Europe/London",
    offsetLabel: "GMT+0/GMT+1",
    hasDST: true
  },
  {
    id: "new-york",
    name: "紐約",
    timeZone: "America/New_York",
    offsetLabel: "GMT-5/GMT-4",
    hasDST: true
  },
  {
    id: "los-angeles",
    name: "洛杉磯",
    timeZone: "America/Los_Angeles",
    offsetLabel: "GMT-8/GMT-7",
    hasDST: true
  },
  {
    id: "paris",
    name: "巴黎",
    timeZone: "Europe/Paris",
    offsetLabel: "GMT+1/GMT+2",
    hasDST: true
  },
  {
    id: "berlin",
    name: "柏林",
    timeZone: "Europe/Berlin",
    offsetLabel: "GMT+1/GMT+2",
    hasDST: true
  },
  {
    id: "moscow",
    name: "莫斯科",
    timeZone: "Europe/Moscow",
    offsetLabel: "GMT+3",
    hasDST: false
  },
  {
    id: "singapore",
    name: "新加坡",
    timeZone: "Asia/Singapore",
    offsetLabel: "GMT+8",
    hasDST: false
  },
  {
    id: "sydney",
    name: "悉尼",
    timeZone: "Australia/Sydney",
    offsetLabel: "GMT+10/GMT+11",
    hasDST: true
  }
];
```

**說明**:

- 總共 10 個城市，預設主要顯示為台北
- 夏令時間支援城市：倫敦、紐約、洛杉磯、巴黎、柏林、悉尼
- 不支援夏令時間城市：台北、東京、莫斯科、新加坡

## 資料持久化

**不需要持久化** - 此功能不儲存使用者偏好設定，每次載入頁面都使用預設配置（台北為主要顯示）。

**未來擴充考量**:

如果需要儲存使用者偏好（記住主要城市選擇），可使用：

- **localStorage**: 儲存 `mainCity.id`
- **Session Storage**: 僅保留本次會話
- **Cookie**: 跨頁面保留設定

範例：

```javascript
// 儲存使用者偏好
function savePreference(cityId) {
  localStorage.setItem('worldclock_main_city', cityId);
}

// 載入使用者偏好
function loadPreference() {
  const savedCityId = localStorage.getItem('worldclock_main_city');
  if (savedCityId) {
    // 設定為主要城市
    const savedCity = cityConfigs.find(c => c.id === savedCityId);
    if (savedCity) {
      return savedCity;
    }
  }
  // 預設返回台北
  return cityConfigs[0];
}
```

## 邊界案例處理

### 1. 時區跨日問題

**場景**: 主要顯示城市與本地時間不在同一天

**處理**: 日期顯示跟隨主要城市的時區，使用該時區的當前日期

```javascript
function getDateInTimezone(timeZone) {
  const formatter = new Intl.DateTimeFormat('zh-TW', {
    timeZone: timeZone,
    year: 'numeric',
    month: '2-digit',
    day: '2-digit'
  });
  return formatter.format(new Date());
}
```

### 2. 夏令時間轉換邊界

**場景**: 在夏令時間轉換的瞬間（如凌晨 2:00）

**處理**: `Intl.DateTimeFormat` 自動處理，時間會正確跳轉或回退

**驗證**: 在夏令時間轉換前後測試時間顯示

### 3. 瀏覽器時區設定錯誤

**場景**: 使用者的系統時區設定不正確

**處理**: 顯示警告訊息，建議使用者檢查系統時間設定

```javascript
function validateSystemTime() {
  const now = new Date();
  const year = now.getFullYear();
  
  // 檢查系統時間是否合理（2020-2030 年之間）
  if (year < 2020 || year > 2030) {
    showWarning('系統時間可能不正確，請檢查您的裝置時間設定');
  }
}
```

### 4. 長時間執行

**場景**: 使用者讓頁面持續運行數小時甚至數天

**處理**:

- 使用 `new Date()` 每次更新時取得最新時間（不依賴計數器）
- 使用 Page Visibility API 在頁面隱藏時停止更新（省電）
- 頁面重新顯示時立即更新時間（補償延遲）

### 5. 快速切換主要時間

**場景**: 使用者快速連續點選多個城市時間卡片

**處理**:

- 使用防抖（debounce）或節流（throttle）限制切換頻率
- 播放切換動畫時禁用點擊（使用 `pointer-events: none`）
- 動畫完成後恢復點擊

```javascript
let isSwitching = false;

function switchMainCity(cityId) {
  if (isSwitching) return; // 防止快速切換
  
  isSwitching = true;
  
  // 執行切換邏輯
  performCitySwitch(cityId);
  
  // 播放動畫（300ms）
  setTimeout(() => {
    isSwitching = false;
  }, 300);
}
```

## 效能考量

### Formatter 實例重用

```javascript
// 建立可重用的 Formatter 快取
const formatterCache = new Map();

function getTimeFormatter(timeZone) {
  const cacheKey = `time-${timeZone}`;
  
  if (!formatterCache.has(cacheKey)) {
    formatterCache.set(cacheKey, new Intl.DateTimeFormat('zh-TW', {
      timeZone: timeZone,
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: false
    }));
  }
  
  return formatterCache.get(cacheKey);
}
```

**優化效果**:

- 避免每秒建立 10 個 Formatter 實例
- 記憶體開銷從 ~2MB 降至 ~200KB
- 更新延遲從 ~20ms 降至 ~5ms

### DOM 更新批次化

```javascript
function updateAllClocks() {
  const now = new Date();
  
  // 批次收集更新
  const updates = [];
  
  // 主要城市
  updates.push({
    element: document.getElementById('main-time'),
    value: getTimeFormatter(clockState.mainCity.timeZone).format(now)
  });
  
  // 次要城市
  clockState.secondaryCities.forEach(city => {
    updates.push({
      element: document.getElementById(`time-${city.id}`),
      value: getTimeFormatter(city.timeZone).format(now)
    });
  });
  
  // 批次執行 DOM 更新（減少 reflow）
  updates.forEach(update => {
    if (update.element) {
      update.element.textContent = update.value;
    }
  });
}
```

## 測試考量

### 單元測試（如適用）

- 測試 `CityTimezone` 驗證邏輯
- 測試 `ClockState` 狀態轉換
- 測試城市切換邏輯（不涉及 DOM）

### 整合測試

- 測試頁面載入時顯示所有 10 個城市
- 測試主要城市預設為台北
- 測試點選城市卡片後主要城市切換
- 測試 HTML 結構符合無障礙標準

### UI 測試（如適用）

- 測試時間每秒更新
- 測試切換動畫播放
- 測試響應式佈局（不同螢幕尺寸）
- 測試鍵盤導覽和 ARIA 支援

---

**資料模型完成日期**: 2025-11-01  
**下一步**: 建立 API contracts（contracts/ 目錄）
