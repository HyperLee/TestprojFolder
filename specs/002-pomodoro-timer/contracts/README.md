# API Contracts: 番茄工作法計時器

本目錄包含番茄工作法計時器的 API 合約文件。

## 文件清單

- [pomodoro-api.md](./pomodoro-api.md) - JavaScript 客戶端 API 規格

## 概述

本專案主要在**客戶端**執行計時邏輯（Vanilla JavaScript），伺服器端僅提供：

1. Razor Pages 渲染（GET `/Pomodoro`）
2. 設定讀寫 API（POST `/Pomodoro/SaveSettings`）
3. 統計記錄 API（POST `/Pomodoro/RecordComplete`）

詳細的客戶端 JavaScript API 設計請參閱 [pomodoro-api.md](./pomodoro-api.md)。
