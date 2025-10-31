# Contracts for World Clock Feature

此目錄包含世界時鐘功能的資料結構合約定義。

## 目的

Contracts 定義了客戶端 JavaScript 和 Razor PageModel 之間的資料結構介面，確保不同元件之間的資料一致性。

## 合約文件

### [world-clock-data.md](world-clock-data.md)

定義世界時鐘的核心資料結構：

- **CityTimezone**: 城市時區配置結構
- **ClockState**: 時鐘狀態管理結構
- **FormattedTime**: 格式化時間輸出結構

這些結構在客戶端 JavaScript（worldclock.js）中實作，並在整合測試中驗證。

## 使用指南

### 客戶端實作

```javascript
// worldclock.js 中使用這些結構
const cityConfig = {
  id: "taipei",
  name: "台北",
  timeZone: "Asia/Taipei",
  offsetLabel: "GMT+8",
  hasDST: false
};

const clockState = {
  mainCity: cityConfig,
  secondaryCities: [...],
  isRunning: true,
  timerId: null,
  lastUpdateTime: Date.now()
};
```

### 測試驗證

```csharp
// WorldClockPageTests.cs 中驗證 HTML 結構
[Fact]
public async Task WorldClock_DisplaysAllCities()
{
    // 驗證頁面包含 10 個城市的時間顯示
    var cityElements = document.QuerySelectorAll(".city-card");
    Assert.Equal(9, cityElements.Length); // 9 個次要城市
    
    var mainClock = document.QuerySelector(".main-clock");
    Assert.NotNull(mainClock); // 1 個主要城市
}
```

## 版本管理

當前版本：**1.0.0**

如果資料結構需要變更：

1. 更新合約文件版本號
2. 在 CHANGELOG 中記錄變更
3. 更新所有使用該結構的程式碼
4. 執行完整測試套件驗證相容性

## 相關文件

- [../data-model.md](../data-model.md): 完整的資料模型說明
- [../spec.md](../spec.md): 功能規格
- [../research.md](../research.md): 技術研究和決策

---

**建立日期**: 2025-11-01  
**最後更新**: 2025-11-01
