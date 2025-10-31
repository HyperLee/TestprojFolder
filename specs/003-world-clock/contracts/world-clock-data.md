# World Clock Data Contracts

**Version**: 1.0.0  
**Date**: 2025-11-01  
**Feature**: 003-world-clock

此文件定義世界時鐘功能的核心資料結構合約，這些結構在客戶端 JavaScript 中實作。

## 合約定義

### 1. CityTimezone

城市時區配置結構。

**TypeScript 介面定義**:

```typescript
interface CityTimezone {
  /** 城市唯一識別符（用於 DOM ID） */
  id: string;
  
  /** 城市顯示名稱（繁體中文） */
  name: string;
  
  /** IANA 時區識別符 */
  timeZone: string;
  
  /** 時區偏移量標籤（如 GMT+8） */
  offsetLabel: string;
  
  /** 是否支援夏令時間 */
  hasDST: boolean;
}
```

**JSON Schema**:

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "required": ["id", "name", "timeZone", "offsetLabel", "hasDST"],
  "properties": {
    "id": {
      "type": "string",
      "pattern": "^[a-z]+(-[a-z]+)*$",
      "description": "城市唯一識別符，小寫字母和連字號"
    },
    "name": {
      "type": "string",
      "minLength": 1,
      "description": "城市顯示名稱"
    },
    "timeZone": {
      "type": "string",
      "pattern": "^[A-Za-z]+/[A-Za-z_]+$",
      "description": "IANA 時區識別符，如 Asia/Taipei"
    },
    "offsetLabel": {
      "type": "string",
      "pattern": "^GMT[+-]\\d{1,2}(/GMT[+-]\\d{1,2})?$",
      "description": "時區偏移量標籤，如 GMT+8 或 GMT-5/GMT-4"
    },
    "hasDST": {
      "type": "boolean",
      "description": "是否支援夏令時間"
    }
  }
}
```

**JavaScript 範例**:

```javascript
const exampleCity = {
  id: "taipei",
  name: "台北",
  timeZone: "Asia/Taipei",
  offsetLabel: "GMT+8",
  hasDST: false
};
```

**驗證函式**:

```javascript
function validateCityTimezone(city) {
  if (!city || typeof city !== 'object') {
    return { valid: false, error: 'City must be an object' };
  }
  
  if (!city.id || typeof city.id !== 'string' || !/^[a-z]+(-[a-z]+)*$/.test(city.id)) {
    return { valid: false, error: 'Invalid id format' };
  }
  
  if (!city.name || typeof city.name !== 'string' || city.name.trim() === '') {
    return { valid: false, error: 'Name is required' };
  }
  
  if (!city.timeZone || typeof city.timeZone !== 'string' || !/^[A-Za-z]+\/[A-Za-z_]+$/.test(city.timeZone)) {
    return { valid: false, error: 'Invalid timeZone format' };
  }
  
  if (!city.offsetLabel || typeof city.offsetLabel !== 'string') {
    return { valid: false, error: 'offsetLabel is required' };
  }
  
  if (typeof city.hasDST !== 'boolean') {
    return { valid: false, error: 'hasDST must be a boolean' };
  }
  
  return { valid: true };
}
```

### 2. ClockState

時鐘狀態管理結構。

**TypeScript 介面定義**:

```typescript
interface ClockState {
  /** 主要顯示的城市 */
  mainCity: CityTimezone;
  
  /** 次要顯示的城市列表（9 個） */
  secondaryCities: CityTimezone[];
  
  /** 時鐘是否正在運行 */
  isRunning: boolean;
  
  /** setInterval 計時器 ID */
  timerId: number | null;
  
  /** 最後更新的時間戳（毫秒） */
  lastUpdateTime: number;
}
```

**JSON Schema**:

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "required": ["mainCity", "secondaryCities", "isRunning", "timerId", "lastUpdateTime"],
  "properties": {
    "mainCity": {
      "$ref": "#/definitions/CityTimezone"
    },
    "secondaryCities": {
      "type": "array",
      "items": {
        "$ref": "#/definitions/CityTimezone"
      },
      "minItems": 9,
      "maxItems": 9
    },
    "isRunning": {
      "type": "boolean"
    },
    "timerId": {
      "type": ["number", "null"]
    },
    "lastUpdateTime": {
      "type": "number",
      "minimum": 0
    }
  }
}
```

**JavaScript 範例**:

```javascript
const exampleState = {
  mainCity: {
    id: "taipei",
    name: "台北",
    timeZone: "Asia/Taipei",
    offsetLabel: "GMT+8",
    hasDST: false
  },
  secondaryCities: [
    { id: "tokyo", name: "東京", timeZone: "Asia/Tokyo", offsetLabel: "GMT+9", hasDST: false },
    // ... 其他 8 個城市
  ],
  isRunning: true,
  timerId: 12345,
  lastUpdateTime: 1698825600000
};
```

**驗證函式**:

```javascript
function validateClockState(state) {
  if (!state || typeof state !== 'object') {
    return { valid: false, error: 'State must be an object' };
  }
  
  const mainCityValidation = validateCityTimezone(state.mainCity);
  if (!mainCityValidation.valid) {
    return { valid: false, error: `Invalid mainCity: ${mainCityValidation.error}` };
  }
  
  if (!Array.isArray(state.secondaryCities) || state.secondaryCities.length !== 9) {
    return { valid: false, error: 'secondaryCities must be an array of 9 cities' };
  }
  
  for (let i = 0; i < state.secondaryCities.length; i++) {
    const cityValidation = validateCityTimezone(state.secondaryCities[i]);
    if (!cityValidation.valid) {
      return { valid: false, error: `Invalid city at index ${i}: ${cityValidation.error}` };
    }
  }
  
  if (typeof state.isRunning !== 'boolean') {
    return { valid: false, error: 'isRunning must be a boolean' };
  }
  
  if (state.timerId !== null && typeof state.timerId !== 'number') {
    return { valid: false, error: 'timerId must be a number or null' };
  }
  
  if (typeof state.lastUpdateTime !== 'number' || state.lastUpdateTime < 0) {
    return { valid: false, error: 'lastUpdateTime must be a non-negative number' };
  }
  
  return { valid: true };
}
```

### 3. FormattedTime

格式化時間輸出結構（運行時計算）。

**TypeScript 介面定義**:

```typescript
interface FormattedTime {
  /** 格式化的時間字串（HH:mm:ss） */
  timeString: string;
  
  /** 格式化的日期字串（YYYY-MM-DD） */
  dateString: string;
  
  /** 當前時區偏移量顯示 */
  offsetDisplay: string;
  
  /** 當前是否為夏令時間 */
  isDST: boolean;
}
```

**JSON Schema**:

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "required": ["timeString", "dateString", "offsetDisplay", "isDST"],
  "properties": {
    "timeString": {
      "type": "string",
      "pattern": "^\\d{2}:\\d{2}:\\d{2}$",
      "description": "24 小時制時間，格式 HH:mm:ss"
    },
    "dateString": {
      "type": "string",
      "pattern": "^\\d{4}-\\d{2}-\\d{2}$",
      "description": "日期格式 YYYY-MM-DD"
    },
    "offsetDisplay": {
      "type": "string",
      "pattern": "^GMT[+-]\\d{1,2}$",
      "description": "時區偏移量顯示，如 GMT+8"
    },
    "isDST": {
      "type": "boolean",
      "description": "是否為夏令時間"
    }
  }
}
```

**JavaScript 範例**:

```javascript
const exampleTime = {
  timeString: "14:30:45",
  dateString: "2025-11-01",
  offsetDisplay: "GMT+8",
  isDST: false
};
```

**驗證函式**:

```javascript
function validateFormattedTime(time) {
  if (!time || typeof time !== 'object') {
    return { valid: false, error: 'Time must be an object' };
  }
  
  if (!time.timeString || !/^\d{2}:\d{2}:\d{2}$/.test(time.timeString)) {
    return { valid: false, error: 'Invalid timeString format (expected HH:mm:ss)' };
  }
  
  if (!time.dateString || !/^\d{4}-\d{2}-\d{2}$/.test(time.dateString)) {
    return { valid: false, error: 'Invalid dateString format (expected YYYY-MM-DD)' };
  }
  
  if (!time.offsetDisplay || !/^GMT[+-]\d{1,2}$/.test(time.offsetDisplay)) {
    return { valid: false, error: 'Invalid offsetDisplay format (expected GMT±N)' };
  }
  
  if (typeof time.isDST !== 'boolean') {
    return { valid: false, error: 'isDST must be a boolean' };
  }
  
  return { valid: true };
}
```

## 常數定義

### 預設城市配置

```javascript
const DEFAULT_CITIES = [
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

// 預設主要城市索引
const DEFAULT_MAIN_CITY_INDEX = 0; // 台北
```

### 配置常數

```javascript
const CONFIG = {
  /** 更新間隔（毫秒） */
  UPDATE_INTERVAL: 1000,
  
  /** 切換動畫持續時間（毫秒） */
  SWITCH_ANIMATION_DURATION: 300,
  
  /** Formatter 語言 */
  FORMATTER_LOCALE: 'zh-TW',
  
  /** 時間格式選項 */
  TIME_FORMAT_OPTIONS: {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    hour12: false
  },
  
  /** 日期格式選項 */
  DATE_FORMAT_OPTIONS: {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit'
  }
};
```

## 使用範例

### 初始化時鐘

```javascript
// 初始化時鐘狀態
function initializeClock() {
  const state = {
    mainCity: DEFAULT_CITIES[DEFAULT_MAIN_CITY_INDEX],
    secondaryCities: DEFAULT_CITIES.filter((_, index) => index !== DEFAULT_MAIN_CITY_INDEX),
    isRunning: false,
    timerId: null,
    lastUpdateTime: Date.now()
  };
  
  // 驗證狀態
  const validation = validateClockState(state);
  if (!validation.valid) {
    console.error('Invalid clock state:', validation.error);
    return null;
  }
  
  return state;
}
```

### 格式化時間

```javascript
// 格式化特定時區的時間
function formatTimeForCity(city) {
  const now = new Date();
  
  const timeFormatter = new Intl.DateTimeFormat(CONFIG.FORMATTER_LOCALE, {
    timeZone: city.timeZone,
    ...CONFIG.TIME_FORMAT_OPTIONS
  });
  
  const dateFormatter = new Intl.DateTimeFormat(CONFIG.FORMATTER_LOCALE, {
    timeZone: city.timeZone,
    ...CONFIG.DATE_FORMAT_OPTIONS
  });
  
  const formattedTime = {
    timeString: timeFormatter.format(now),
    dateString: dateFormatter.format(now),
    offsetDisplay: city.offsetLabel.split('/')[0], // 簡化顯示
    isDST: false // 需要額外邏輯判斷
  };
  
  // 驗證格式化結果
  const validation = validateFormattedTime(formattedTime);
  if (!validation.valid) {
    console.error('Invalid formatted time:', validation.error);
    return null;
  }
  
  return formattedTime;
}
```

### 切換主要城市

```javascript
// 切換主要顯示城市
function switchMainCity(state, newMainCityId) {
  // 驗證當前狀態
  const stateValidation = validateClockState(state);
  if (!stateValidation.valid) {
    throw new Error(`Invalid state: ${stateValidation.error}`);
  }
  
  // 找到新的主要城市
  const newMainCityIndex = state.secondaryCities.findIndex(city => city.id === newMainCityId);
  if (newMainCityIndex === -1) {
    throw new Error(`City not found: ${newMainCityId}`);
  }
  
  const newMainCity = state.secondaryCities[newMainCityIndex];
  
  // 建立新狀態
  const newState = {
    ...state,
    mainCity: newMainCity,
    secondaryCities: [
      ...state.secondaryCities.slice(0, newMainCityIndex),
      state.mainCity,
      ...state.secondaryCities.slice(newMainCityIndex + 1)
    ],
    lastUpdateTime: Date.now()
  };
  
  // 驗證新狀態
  const newStateValidation = validateClockState(newState);
  if (!newStateValidation.valid) {
    throw new Error(`Invalid new state: ${newStateValidation.error}`);
  }
  
  return newState;
}
```

## 測試契約

### 單元測試範例

```javascript
describe('CityTimezone Contract', () => {
  test('validates correct city timezone', () => {
    const validCity = {
      id: "taipei",
      name: "台北",
      timeZone: "Asia/Taipei",
      offsetLabel: "GMT+8",
      hasDST: false
    };
    
    const result = validateCityTimezone(validCity);
    expect(result.valid).toBe(true);
  });
  
  test('rejects invalid id format', () => {
    const invalidCity = {
      id: "Taipei", // 應該是小寫
      name: "台北",
      timeZone: "Asia/Taipei",
      offsetLabel: "GMT+8",
      hasDST: false
    };
    
    const result = validateCityTimezone(invalidCity);
    expect(result.valid).toBe(false);
    expect(result.error).toContain('Invalid id format');
  });
});
```

### 整合測試範例（C# xUnit）

```csharp
[Fact]
public async Task WorldClock_InitialState_DisplaysTaipeiAsMainCity()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/WorldClock");
    response.EnsureSuccessStatusCode();
    
    var content = await response.Content.ReadAsStringAsync();
    var document = await ParseHtmlAsync(content);
    
    // Assert - 驗證主要城市為台北
    var mainClock = document.QuerySelector(".main-clock");
    Assert.NotNull(mainClock);
    
    var cityName = mainClock.QuerySelector(".city-name")?.TextContent;
    Assert.Equal("台北", cityName);
    
    var timezone = mainClock.QuerySelector(".timezone")?.TextContent;
    Assert.Contains("GMT+8", timezone);
}
```

## 版本變更記錄

### Version 1.0.0 (2025-11-01)

- 初始版本
- 定義 CityTimezone、ClockState、FormattedTime 結構
- 定義驗證函式和常數
- 提供使用範例和測試契約

---

**文件完成日期**: 2025-11-01  
**維護者**: 開發團隊  
**審查週期**: 每次資料結構變更時
