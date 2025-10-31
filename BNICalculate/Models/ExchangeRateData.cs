using System.ComponentModel.DataAnnotations;

namespace BNICalculate.Models;

/// <summary>
/// 包裝所有貨幣的匯率資料及更新時間
/// </summary>
public class ExchangeRateData
{
    /// <summary>
    /// 所有貨幣的匯率清單
    /// </summary>
    [Required(ErrorMessage = "匯率清單為必填")]
    [MinLength(1, ErrorMessage = "至少需要一筆匯率資料")]
    public List<ExchangeRate> Rates { get; set; } = new();

    /// <summary>
    /// 資料取得時間（UTC+8）
    /// </summary>
    [Required(ErrorMessage = "資料取得時間為必填")]
    public DateTime LastFetchTime { get; set; }

    /// <summary>
    /// 資料來源
    /// </summary>
    [Required(ErrorMessage = "資料來源為必填")]
    public string DataSource { get; set; } = "台灣銀行";

    /// <summary>
    /// 檢查資料是否過期（超過 24 小時）
    /// </summary>
    /// <returns>資料是否過期</returns>
    public bool IsStale()
    {
        var now = DateTime.Now;
        var age = now - LastFetchTime;
        return age.TotalHours > 24;
    }

    /// <summary>
    /// 根據貨幣代碼取得匯率
    /// </summary>
    /// <param name="currencyCode">貨幣代碼（如 "USD"）</param>
    /// <returns>對應的匯率資料，若不存在則返回 null</returns>
    public ExchangeRate? GetRate(string currencyCode)
    {
        return Rates.FirstOrDefault(r => 
            r.CurrencyCode.Equals(currencyCode, StringComparison.OrdinalIgnoreCase));
    }
}
