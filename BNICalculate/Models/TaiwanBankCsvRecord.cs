using CsvHelper.Configuration.Attributes;

namespace BNICalculate.Models;

/// <summary>
/// 台灣銀行 CSV 資料記錄
/// </summary>
public class TaiwanBankCsvRecord
{
    /// <summary>
    /// 貨幣代碼（如 USD, JPY）
    /// </summary>
    [Name("幣別")]
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// 貨幣名稱（如美金）
    /// </summary>
    [Name("幣別名稱")]
    public string CurrencyName { get; set; } = string.Empty;

    /// <summary>
    /// 現金買入匯率
    /// </summary>
    [Name("本行現金買入")]
    public string CashBuyRate { get; set; } = string.Empty;

    /// <summary>
    /// 現金賣出匯率
    /// </summary>
    [Name("本行現金賣出")]
    public string CashSellRate { get; set; } = string.Empty;
}
