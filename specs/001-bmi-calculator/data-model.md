# Data Model: BMI 計算器

**Feature**: 001-bmi-calculator  
**Date**: 2025年10月31日  
**Purpose**: 定義資料結構和實體關係

## 概述

BMI 計算器是無狀態的客戶端應用程式，**不需要資料持久化或後端資料模型**。所有資料僅存在於瀏覽器的 DOM 和 JavaScript 記憶體中，頁面重新整理後資料消失。

## 實體定義

### 1. 使用者輸入（UserInput）

客戶端 JavaScript 物件，儲存使用者輸入的資料。

**屬性**:

| 屬性名稱 | 型別 | 單位 | 驗證規則 | 說明 |
|---------|------|------|----------|------|
| `height` | `number` | 公尺 (m) | > 0, 數字 | 使用者身高 |
| `weight` | `number` | 公斤 (kg) | > 0, 數字 | 使用者體重 |

**JavaScript 表示**:

```javascript
const userInput = {
    height: 1.75,  // 公尺
    weight: 70     // 公斤
};
```

**來源**: HTML 表單輸入欄位
- `<input id="height" type="number" />`
- `<input id="weight" type="number" />`

---

### 2. BMI 計算結果（BMIResult）

客戶端 JavaScript 物件，儲存計算後的 BMI 值和分類。

**屬性**:

| 屬性名稱 | 型別 | 格式 | 說明 |
|---------|------|------|------|
| `bmi` | `number` | 小數點一位 | 計算得出的 BMI 值 |
| `category` | `string` | 文字 | WHO 體重分類名稱 |

**JavaScript 表示**:

```javascript
const bmiResult = {
    bmi: 22.9,
    category: "正常"
};
```

**計算公式**:

```javascript
bmi = weight / (height * height)
bmi = Math.round(bmi * 10) / 10  // 四捨五入至小數點一位
```

**分類對應表** (WHO 標準):

| BMI 範圍 | 分類 |
|----------|------|
| < 18.5 | 過輕 |
| 18.5 - 23.9 | 正常 |
| 24.0 - 26.9 | 過重 |
| 27.0 - 29.9 | 輕度肥胖 |
| 30.0 - 34.9 | 中度肥胖 |
| ≥ 35.0 | 重度肥胖 |

---

### 3. 驗證錯誤（ValidationError）

客戶端 JavaScript 物件，儲存驗證錯誤訊息。

**屬性**:

| 屬性名稱 | 型別 | 說明 |
|---------|------|------|
| `field` | `string` | 欄位名稱（"height" 或 "weight"） |
| `message` | `string` | 繁體中文錯誤訊息 |

**JavaScript 表示**:

```javascript
const validationError = {
    field: "height",
    message: "請輸入有效的身高值（大於 0）"
};
```

**錯誤訊息清單**:

| 驗證情境 | 錯誤訊息 |
|----------|----------|
| 空值 | 「請輸入完整的身高和體重資料」 |
| 非數字 | 「請輸入數字」 |
| 零或負數 | 「請輸入有效的{欄位}值（大於 0）」 |

---

## 資料流程

### 輸入 → 驗證 → 計算 → 顯示

```text
1. 使用者輸入
   ↓
   DOM Input Elements (#height, #weight)
   ↓
2. 觸發驗證（JavaScript）
   ↓
   驗證規則：非空、數字、正數
   ↓
   [失敗] → 顯示錯誤訊息於欄位下方
   [成功] → 繼續
   ↓
3. 計算 BMI（JavaScript）
   ↓
   bmi = weight / (height²)
   category = getBMICategory(bmi)
   ↓
4. 顯示結果（更新 DOM）
   ↓
   顯示於 #result 區域:
   - "BMI: 22.9"
   - "分類: 正常"
```

### 清除流程

```text
使用者點擊「清除」按鈕
   ↓
JavaScript 清空:
- Input 欄位值
- 錯誤訊息
- 結果顯示區域
```

---

## DOM 元素對應

| 資料 | HTML 元素 | ID/Class |
|------|-----------|----------|
| 身高輸入 | `<input type="number">` | `#height` |
| 體重輸入 | `<input type="number">` | `#weight` |
| 身高錯誤 | `<span class="error-message">` | `.error-message` (動態插入) |
| 體重錯誤 | `<span class="error-message">` | `.error-message` (動態插入) |
| BMI 值顯示 | `<div>` | `#bmi-value` |
| 分類顯示 | `<div>` | `#bmi-category` |
| 計算按鈕 | `<button>` | `#calculate-btn` |
| 清除按鈕 | `<button>` | `#clear-btn` |

---

## 狀態管理

**狀態存儲**: 無

- 頁面無全域狀態或 Session Storage
- 所有資料存在於 DOM 元素和臨時 JavaScript 變數中
- 頁面重新整理後資料消失（符合需求）

**為什麼不需要狀態管理？**

1. 單一頁面，無導覽
2. 無使用者帳號或個人化設定
3. 規格未要求儲存歷史記錄

---

## 資料驗證邏輯

### JavaScript 驗證函數

```javascript
function validateInput(height, weight) {
    const errors = [];
    
    // 檢查空值
    if (!height || !weight) {
        errors.push({ 
            field: "general", 
            message: "請輸入完整的身高和體重資料" 
        });
        return errors;
    }
    
    // 檢查數字
    if (isNaN(height)) {
        errors.push({ 
            field: "height", 
            message: "請輸入數字" 
        });
    }
    if (isNaN(weight)) {
        errors.push({ 
            field: "weight", 
            message: "請輸入數字" 
        });
    }
    
    // 檢查正數
    if (height <= 0) {
        errors.push({ 
            field: "height", 
            message: "請輸入有效的身高值（大於 0）" 
        });
    }
    if (weight <= 0) {
        errors.push({ 
            field: "weight", 
            message: "請輸入有效的體重值（大於 0）" 
        });
    }
    
    return errors;
}
```

### BMI 分類函數

```javascript
function getBMICategory(bmi) {
    if (bmi < 18.5) return "過輕";
    if (bmi < 24.0) return "正常";
    if (bmi < 27.0) return "過重";
    if (bmi < 30.0) return "輕度肥胖";
    if (bmi < 35.0) return "中度肥胖";
    return "重度肥胖";
}
```

---

## 測試資料

### 測試案例

| 身高 (m) | 體重 (kg) | 預期 BMI | 預期分類 |
|----------|----------|----------|----------|
| 1.75 | 70 | 22.9 | 正常 |
| 1.60 | 45 | 17.6 | 過輕 |
| 1.70 | 90 | 31.1 | 中度肥胖 |
| 1.80 | 65 | 20.1 | 正常 |
| 1.65 | 80 | 29.4 | 輕度肥胖 |

### 邊界測試

| 輸入 | 預期結果 |
|------|----------|
| 身高 = 0 | 錯誤:「請輸入有效的身高值（大於 0）」 |
| 體重 = -5 | 錯誤:「請輸入有效的體重值（大於 0）」 |
| 身高 = "abc" | 錯誤:「請輸入數字」 |
| 空值 | 錯誤:「請輸入完整的身高和體重資料」 |
| 身高 = 3, 體重 = 500 | BMI 計算正常（無上限限制） |

---

## 總結

- **無後端資料模型**: 所有邏輯在客戶端
- **無資料持久化**: 符合規格需求
- **簡單資料結構**: 僅 3 個 JavaScript 物件
- **即時計算**: 無 API 呼叫或非同步操作

**下一步**: 建立 quickstart.md 和更新 Copilot 指令檔
