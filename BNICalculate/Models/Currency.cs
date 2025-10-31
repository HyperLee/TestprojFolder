using System.ComponentModel.DataAnnotations;

namespace BNICalculate.Models;

/// <summary>
/// 支援的貨幣類型
/// </summary>
public enum Currency
{
    /// <summary>美元</summary>
    [Display(Name = "美元 (USD)")]
    USD,

    /// <summary>日圓</summary>
    [Display(Name = "日圓 (JPY)")]
    JPY,

    /// <summary>人民幣</summary>
    [Display(Name = "人民幣 (CNY)")]
    CNY,

    /// <summary>歐元</summary>
    [Display(Name = "歐元 (EUR)")]
    EUR,

    /// <summary>英鎊</summary>
    [Display(Name = "英鎊 (GBP)")]
    GBP,

    /// <summary>港幣</summary>
    [Display(Name = "港幣 (HKD)")]
    HKD,

    /// <summary>澳幣</summary>
    [Display(Name = "澳幣 (AUD)")]
    AUD
}

/// <summary>
/// Currency 列舉的擴充方法
/// </summary>
public static class CurrencyExtensions
{
    /// <summary>
    /// 取得貨幣顯示名稱
    /// </summary>
    /// <param name="currency">貨幣列舉值</param>
    /// <returns>貨幣顯示名稱</returns>
    public static string GetDisplayName(this Currency currency)
    {
        var type = currency.GetType();
        var member = type.GetMember(currency.ToString());
        var attributes = member[0].GetCustomAttributes(typeof(DisplayAttribute), false);
        return attributes.Length > 0 ? ((DisplayAttribute)attributes[0]).Name ?? currency.ToString() : currency.ToString();
    }

    /// <summary>
    /// 將貨幣代碼字串轉換為 Currency 列舉
    /// </summary>
    /// <param name="code">貨幣代碼（如 "USD", "JPY"）</param>
    /// <returns>對應的 Currency 列舉值</returns>
    /// <exception cref="ArgumentException">不支援的貨幣代碼</exception>
    public static Currency FromCode(string code)
    {
        if (Enum.TryParse<Currency>(code, ignoreCase: true, out var currency))
        {
            return currency;
        }
        throw new ArgumentException($"不支援的貨幣代碼: {code}", nameof(code));
    }
}
