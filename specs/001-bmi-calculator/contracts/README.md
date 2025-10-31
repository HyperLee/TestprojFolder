# API Contracts

**Feature**: 001-bmi-calculator  
**Date**: 2025年10月31日

## 說明

此功能**不包含後端 API**。所有計算邏輯在客戶端 JavaScript 執行，無需伺服器端端點。

## 原因

- BMI 計算是純客戶端操作
- 無資料持久化需求
- 無需使用者認證或授權
- 符合「簡單」和「簡潔」原則

因此，`contracts/` 目錄保持為空。
