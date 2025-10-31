#!/bin/bash

# 測試台幣轉外幣
echo "測試台幣轉外幣計算..."
curl -s -X POST http://localhost:5087/CurrencyConverter?handler=CalculateTwdToForeign \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "TwdAmount=10000&SelectedCurrency=USD" \
  | grep -o "兌換後金額：.*USD" | head -1

echo ""
echo "測試完成。請在瀏覽器開啟 http://localhost:5087/CurrencyConverter 進行實際測試。"
