using System.ComponentModel.DataAnnotations;

namespace BNICalculate.Models;

/// <summary>
/// 匯率計算請求
/// </summary>
public class CalculationRequest
{
    /// <summary>
    /// 金額
    /// </summary>
    [Required(ErrorMessage = "金額為必填")]
    [Range(0.01, double.MaxValue, ErrorMessage = "金額必須為正數")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 貨幣代碼（如 USD）
    /// </summary>
    [Required(ErrorMessage = "貨幣代碼為必填")]
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>
    /// 驗證請求是否有效
    /// </summary>
    /// <returns>請求是否有效</returns>
    public bool IsValid()
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(this);
        return Validator.TryValidateObject(this, context, validationResults, true);
    }
}
