# Research: 世界時鐘技術選擇與最佳實務

**Date**: 2025-11-01  
**Feature**: 003-world-clock  
**Purpose**: 研究並確定世界時鐘功能的技術實作方案，解決時區處理、夏令時間、效能優化等關鍵技術問題

## 研究目標

根據 Technical Context 中的需求，需要研究以下關鍵領域：

1. **JavaScript 時區處理**: 如何正確處理多時區和夏令時間
2. **即時更新機制**: 每秒更新時間的最佳實務（效能和精確度）
3. **客戶端時間計算**: JavaScript Date API vs Intl.DateTimeFormat vs 第三方函式庫
4. **無障礙設計**: 動態更新內容的無障礙支援
5. **效能優化**: 長時間運行下避免記憶體洩漏和累積誤差

## 研究發現

### 1. JavaScript 時區處理

#### Decision: 使用原生 Intl.DateTimeFormat API

**Rationale**:

- `Intl.DateTimeFormat` 是瀏覽器原生支援的國際化 API，自動處理夏令時間轉換
- 支援所有現代瀏覽器（Chrome、Firefox、Safari、Edge）
- 無需第三方函式庫，減少打包大小和相依性
- 效能優異，由瀏覽器引擎優化
- 提供完整的時區資料庫（IANA Time Zone Database）

**Alternatives considered**:

- **Moment.js**: ❌ 已不再維護，打包大小大（67KB minified），不推薦用於新專案
- **date-fns-tz**: ✅ 現代化、模組化，但增加額外相依性（~20KB），對於我們的簡單需求過度設計
- **Luxon**: ✅ Moment.js 的繼任者，功能強大，但對於純時區顯示來說過於複雜（~70KB）
- **Day.js**: ⚠️ 輕量（2KB），但時區支援需要額外插件，功能不如原生 API 完整

**Implementation approach**:

```javascript
// 使用 Intl.DateTimeFormat 取得特定時區的時間
const formatter = new Intl.DateTimeFormat('zh-TW', {
  timeZone: 'Asia/Taipei',
  hour: '2-digit',
  minute: '2-digit',
  second: '2-digit',
  hour12: false
});

const taipeiTime = formatter.format(new Date());
```

**時區識別符** (IANA Time Zone Database):

- 台北: `Asia/Taipei`
- 東京: `Asia/Tokyo`
- 倫敦: `Europe/London`
- 紐約: `America/New_York`
- 洛杉磯: `America/Los_Angeles`
- 巴黎: `Europe/Paris`
- 柏林: `Europe/Berlin`
- 莫斯科: `Europe/Moscow`
- 新加坡: `Asia/Singapore`
- 悉尼: `Australia/Sydney`

### 2. 即時更新機制

#### Decision: 使用 `setInterval` 搭配準確的時間戳計算

**Rationale**:

- `setInterval` 是標準的定時器 API，瀏覽器廣泛支援
- 配合 `Date.now()` 計算實際經過時間，避免累積誤差
- 每秒更新一次（1000ms 間隔）平衡了精確度和效能
- 實作簡單，易於測試和維護

**Alternatives considered**:

- **setTimeout 遞迴呼叫**: ✅ 更精確（可動態調整間隔），但實作複雜度較高，對於秒級更新無明顯優勢
- **requestAnimationFrame**: ❌ 設計用於動畫（60fps），每秒 60 次呼叫造成不必要的效能開銷
- **Web Workers**: ❌ 過度設計，主執行緒的 setInterval 已足夠，Worker 增加複雜度和通訊開銷

**Implementation approach**:

```javascript
let timerId = null;

function startClock() {
  // 立即更新一次
  updateAllClocks();
  
  // 每秒更新
  timerId = setInterval(() => {
    updateAllClocks();
  }, 1000);
}

function stopClock() {
  if (timerId) {
    clearInterval(timerId);
    timerId = null;
  }
}

// 頁面卸載時清理
window.addEventListener('beforeunload', stopClock);
```

**避免累積誤差策略**:

- 每次更新都使用 `new Date()` 取得當前系統時間
- 不依賴計數器累加（避免 setInterval 的漂移問題）
- 使用 `Intl.DateTimeFormat` 的實時計算確保準確性

### 3. 客戶端時間計算最佳實務

#### Decision: 組合使用 Date API 和 Intl.DateTimeFormat

**Rationale**:

- **Date API**: 取得當前時間戳和基礎時間操作
- **Intl.DateTimeFormat**: 處理時區轉換和格式化
- **分離關注點**: 時間取得（Date）vs 時區轉換（Intl）
- **效能優化**: 重用 DateTimeFormat 實例（避免重複建立）

**Implementation approach**:

```javascript
// 建立可重用的 formatter 實例（效能優化）
const formatters = {
  'Asia/Taipei': new Intl.DateTimeFormat('zh-TW', {
    timeZone: 'Asia/Taipei',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    hour12: false
  }),
  // ... 其他城市
};

// 取得時區偏移量顯示（如 GMT+8）
function getTimezoneOffset(timeZone) {
  const now = new Date();
  const formatter = new Intl.DateTimeFormat('en-US', {
    timeZone: timeZone,
    timeZoneName: 'short'
  });
  
  const parts = formatter.formatToParts(now);
  const tzName = parts.find(p => p.type === 'timeZoneName').value;
  
  return tzName; // 例如 "GMT+8"
}

// 取得日期（跟隨時區）
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

**夏令時間處理**:

- `Intl.DateTimeFormat` 自動處理夏令時間轉換
- 時區偏移量會根據當前日期動態變化
- 無需手動判斷或調整（瀏覽器內建支援）

### 4. 無障礙設計最佳實務

#### Decision: 使用 ARIA Live Regions 搭配語意化 HTML

**Rationale**:

- 動態更新內容需要通知螢幕閱讀器使用者
- `aria-live="polite"` 在更新完成後通知，不打斷當前操作
- 語意化 HTML（`<time>` 元素）提供更好的語意
- 鍵盤導覽支援（Tab、Enter、Space）
- 符合 WCAG 2.1 Level AA 標準

**Implementation approach**:

```html
<!-- 主要時間區域 -->
<div class="main-clock" role="region" aria-labelledby="main-clock-label">
  <h2 id="main-clock-label" class="visually-hidden">主要時間顯示</h2>
  <div class="city-name" aria-label="城市名稱">台北</div>
  <time class="time-display" aria-live="polite" aria-atomic="true">
    14:30:45
  </time>
  <div class="timezone" aria-label="時區">GMT+8</div>
  <div class="date-display" aria-label="日期">2025-11-01</div>
</div>

<!-- 城市時間卡片（可點選） -->
<button class="city-card" 
        role="button" 
        aria-label="切換到東京時間，當前時間 15:30:45"
        tabindex="0">
  <div class="city-name">東京</div>
  <time class="time-display" aria-live="off">15:30:45</time>
  <div class="timezone">GMT+9</div>
</button>
```

**無障礙關鍵點**:

1. **ARIA Live Regions**: 主要時間使用 `aria-live="polite"`，次要時間使用 `aria-live="off"` 避免過多通知
2. **語意化元素**: 使用 `<time>` 元素標記時間，`<button>` 元素標記可點選卡片
3. **鍵盤導覽**: 所有互動元素支援 Tab 鍵導覽和 Enter/Space 觸發
4. **視覺隱藏標題**: 使用 `.visually-hidden` 提供螢幕閱讀器標題
5. **動態 aria-label**: 點選卡片時更新 aria-label 描述當前狀態
6. **對比度**: 確保文字與背景對比度達到 4.5:1（WCAG AA）

**Alternatives considered**:

- **純 div 結構**: ❌ 缺乏語意，螢幕閱讀器無法理解內容意義
- **aria-live="assertive"**: ❌ 過於激進，每秒更新會嚴重干擾使用者
- **不使用 Live Regions**: ❌ 視障使用者無法得知時間更新

### 5. 效能優化與記憶體管理

#### Decision: 事件監聽器清理 + Formatter 實例重用 + Visibility API

**Rationale**:

- 防止記憶體洩漏：頁面卸載時清理計時器和事件監聽器
- 效能優化：重用 `DateTimeFormat` 實例避免重複建立（每次建立成本約 1-2ms）
- 省電優化：使用 Page Visibility API 在頁面隱藏時停止更新

**Implementation approach**:

```javascript
// 1. Formatter 實例重用（效能優化）
const cityFormatters = new Map();

function getFormatter(timeZone) {
  if (!cityFormatters.has(timeZone)) {
    cityFormatters.set(timeZone, new Intl.DateTimeFormat('zh-TW', {
      timeZone: timeZone,
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: false
    }));
  }
  return cityFormatters.get(timeZone);
}

// 2. 計時器清理（記憶體管理）
let timerId = null;

function cleanup() {
  if (timerId) {
    clearInterval(timerId);
    timerId = null;
  }
}

window.addEventListener('beforeunload', cleanup);
window.addEventListener('pagehide', cleanup);

// 3. Visibility API（省電優化）
document.addEventListener('visibilitychange', function() {
  if (document.hidden) {
    // 頁面隱藏時停止更新
    cleanup();
  } else {
    // 頁面顯示時恢復更新
    startClock();
  }
});

// 4. 避免 DOM 操作開銷
function updateAllClocks() {
  const now = new Date();
  
  // 批次更新 DOM（使用 DocumentFragment 或 直接更新 textContent）
  cities.forEach(city => {
    const timeElement = document.getElementById(`time-${city.id}`);
    if (timeElement) {
      const formatter = getFormatter(city.timeZone);
      timeElement.textContent = formatter.format(now);
    }
  });
}
```

**效能測量**:

- 初始載入時間：測量 DOMContentLoaded 到首次渲染的時間
- 更新延遲：測量 `updateAllClocks()` 執行時間（目標 <10ms）
- 記憶體使用：使用 Chrome DevTools Performance Monitor 監控記憶體趨勢
- 長時間執行測試：運行 24 小時驗證無記憶體洩漏

**Alternatives considered**:

- **不清理計時器**: ❌ 單頁應用中切換頁面會累積計時器，造成記憶體洩漏
- **每次建立新 Formatter**: ❌ 每秒建立 10 個 Formatter 實例，效能開銷大（~10-20ms）
- **requestAnimationFrame**: ❌ 60fps 更新過於頻繁，CPU 使用率高，電池消耗大

### 6. 視覺設計實作

#### Decision: CSS Grid + CSS Variables + Transitions

**Rationale**:

- **CSS Grid**: 響應式網格佈局，自動適應不同螢幕尺寸
- **CSS Variables**: 集中管理色彩和字體規格，易於維護
- **CSS Transitions**: 平滑的 hover 和點選動畫效果
- **Bootstrap 5 整合**: 使用現有的 Bootstrap utilities 和 grid system

**Implementation approach**:

```css
/* CSS Variables（色彩配置） */
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
}

/* 網格佈局（響應式） */
.city-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
  margin-top: 2rem;
}

/* 城市卡片 */
.city-card {
  background: var(--card-bg);
  border: 2px solid transparent;
  border-radius: 8px;
  padding: 1rem;
  cursor: pointer;
  transition: all 0.2s ease;
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

/* 字體規格 */
.city-name {
  font-size: 16px;
  font-weight: 500;
  color: var(--text-gray);
}

.time-display.main {
  font-size: 48px;
  font-weight: 700;
  font-family: 'Courier New', monospace;
  color: var(--text-dark);
}

.time-display.city {
  font-size: 24px;
  font-weight: 600;
  font-family: 'Courier New', monospace;
  color: var(--text-dark);
}

.date-display {
  font-size: 14px;
  font-weight: 400;
  color: var(--text-light);
}
```

**響應式斷點**:

```css
/* 手機（< 768px）：單列 */
@media (max-width: 767px) {
  .city-grid {
    grid-template-columns: 1fr;
  }
  
  .main-clock .time-display {
    font-size: 36px;
  }
}

/* 平板（768px - 1199px）：2 列 */
@media (min-width: 768px) and (max-width: 1199px) {
  .city-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

/* 桌面（>= 1200px）：3 列 */
@media (min-width: 1200px) {
  .city-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}
```

## 技術風險與緩解策略

### 風險 1: 瀏覽器時區資料過時

**風險**: 舊版瀏覽器的 IANA 時區資料庫可能不包含最新的夏令時間規則變更

**緩解策略**:

- 目標瀏覽器版本：Chrome 92+、Firefox 90+、Safari 14+、Edge 92+（2021 年後版本）
- 提供 polyfill 或降級方案（顯示警告訊息）
- 在文件中說明支援的瀏覽器版本

### 風險 2: 長時間運行的精確度

**風險**: `setInterval` 在瀏覽器節能模式或背景執行時可能延遲或停止

**緩解策略**:

- 使用 Page Visibility API 檢測頁面顯示狀態
- 頁面重新顯示時立即更新時間（補償延遲）
- 每次更新使用 `new Date()` 而非計數器累加

### 風險 3: 效能問題（低階裝置）

**風險**: 每秒更新 10 個時間在低階行動裝置上可能造成卡頓

**緩解策略**:

- 批次更新 DOM（避免多次 reflow）
- 使用 `textContent` 而非 `innerHTML`（效能更好）
- 重用 Formatter 實例（避免重複建立）
- 使用 Chrome DevTools Performance 驗證效能

### 風險 4: 無障礙測試覆蓋不足

**風險**: ARIA Live Regions 在不同螢幕閱讀器上行為不一致

**緩解策略**:

- 使用 NVDA（Windows）和 VoiceOver（macOS）測試
- 使用 axe DevTools 自動化無障礙測試
- 提供鍵盤導覽測試案例
- 參考 WCAG 2.1 指引和 ARIA Authoring Practices

## 技術決策總結

| 決策領域 | 選擇方案 | 主要理由 |
|---------|---------|---------|
| 時區處理 | Intl.DateTimeFormat | 原生支援、自動處理夏令時間、無額外相依性 |
| 更新機制 | setInterval | 簡單可靠、瀏覽器廣泛支援、易於測試 |
| 日期/時間 API | Date + Intl | 分離關注點、效能優化、標準化 |
| 無障礙設計 | ARIA Live + 語意化 HTML | 符合 WCAG AA、螢幕閱讀器支援 |
| 效能優化 | Formatter 重用 + Visibility API | 減少記憶體開銷、省電優化 |
| 視覺設計 | CSS Grid + Variables | 響應式佈局、易於維護、Bootstrap 整合 |

## 參考資源

- [MDN: Intl.DateTimeFormat](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Intl/DateTimeFormat)
- [MDN: ARIA Live Regions](https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/ARIA_Live_Regions)
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
- [IANA Time Zone Database](https://www.iana.org/time-zones)
- [Page Visibility API](https://developer.mozilla.org/en-US/docs/Web/API/Page_Visibility_API)
- [Web Performance Best Practices](https://web.dev/performance/)

---

**研究完成日期**: 2025-11-01  
**下一步**: Phase 1 - Design & Contracts（建立 data-model.md 和 contracts）
