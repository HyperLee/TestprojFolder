using CsvHelper.Configuration.Attributes;

namespace BNICalculate.Models;

/// <summary>
/// 台灣銀行 CSV 資料記錄
/// 對應 CSV 格式（買入和賣出在同一行）：
/// 幣別, 匯率(買入), 現金(買入), 即期(買入), ..., 匯率(賣出), 現金(賣出), 即期(賣出), ...
/// 範例：USD, 本行買入, 30.33000, 30.65500, ..., 本行賣出, 31.00000, 30.80500, ...
/// </summary>
public class TaiwanBankCsvRecord
{
    /// <summary>
    /// 貨幣代碼（如 USD, JPY）
    /// </summary>
    [Index(0)]
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// 現金買入匯率（第 2 個欄位，索引 2）
    /// </summary>
    [Index(2)]
    public string CashBuyRate { get; set; } = string.Empty;

    /// <summary>
    /// 現金賣出匯率（第 12 個欄位，索引 12）
    /// </summary>
    [Index(12)]
    public string CashSellRate { get; set; } = string.Empty;
}
