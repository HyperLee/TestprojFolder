using System.ComponentModel.DataAnnotations;

namespace BNICalculate.Models;

/// <summary>
/// 代表特定貨幣的匯率資訊
/// </summary>
public class ExchangeRate
{
    /// <summary>
    /// 貨幣代碼（如 USD, JPY）
    /// </summary>
    [Required(ErrorMessage = "貨幣代碼為必填")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "貨幣代碼必須為3個字元")]
    [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "貨幣代碼必須為3個大寫英文字母")]
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// 貨幣名稱（如美元、日圓）
    /// </summary>
    [Required(ErrorMessage = "貨幣名稱為必填")]
    [StringLength(50, ErrorMessage = "貨幣名稱長度不得超過50個字元")]
    public string CurrencyName { get; set; } = string.Empty;

    /// <summary>
    /// 現金買入匯率（銀行買入外幣的價格）
    /// </summary>
    [Required(ErrorMessage = "現金買入匯率為必填")]
    [Range(0.000001, 999999, ErrorMessage = "匯率必須為正數")]
    public decimal CashBuyRate { get; set; }

    /// <summary>
    /// 現金賣出匯率（銀行賣出外幣的價格）
    /// </summary>
    [Required(ErrorMessage = "現金賣出匯率為必填")]
    [Range(0.000001, 999999, ErrorMessage = "匯率必須為正數")]
    public decimal CashSellRate { get; set; }

    /// <summary>
    /// 最後更新時間（UTC+8）
    /// </summary>
    [Required(ErrorMessage = "最後更新時間為必填")]
    public DateTime LastUpdated { get; set; }
}
