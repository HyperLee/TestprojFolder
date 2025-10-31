namespace BNICalculate.Models;

/// <summary>
/// 匯率計算結果
/// </summary>
public class CalculationResult
{
    /// <summary>
    /// 來源金額
    /// </summary>
    public decimal SourceAmount { get; set; }

    /// <summary>
    /// 來源貨幣代碼
    /// </summary>
    public string SourceCurrency { get; set; } = string.Empty;

    /// <summary>
    /// 目標金額（保留6位小數）
    /// </summary>
    public decimal TargetAmount { get; set; }

    /// <summary>
    /// 目標貨幣代碼
    /// </summary>
    public string TargetCurrency { get; set; } = string.Empty;

    /// <summary>
    /// 使用的匯率
    /// </summary>
    public decimal ExchangeRate { get; set; }

    /// <summary>
    /// 計算時間
    /// </summary>
    public DateTime CalculatedAt { get; set; }

    /// <summary>
    /// 取得格式化的計算結果
    /// </summary>
    /// <returns>格式化的計算結果字串</returns>
    public string GetFormattedResult()
    {
        var sourceFormatted = SourceAmount.ToString("N" + (SourceCurrency == "TWD" ? "0" : "2"));
        var targetFormatted = TargetAmount.ToString("N6");

        return $"{sourceFormatted} {SourceCurrency} = {targetFormatted} {TargetCurrency}";
    }
}
