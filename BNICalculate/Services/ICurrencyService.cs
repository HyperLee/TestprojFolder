using BNICalculate.Models;

namespace BNICalculate.Services;

/// <summary>
/// 匯率計算服務介面
/// </summary>
public interface ICurrencyService
{
    /// <summary>
    /// 計算台幣轉外幣金額
    /// </summary>
    /// <param name="twdAmount">台幣金額</param>
    /// <param name="targetCurrency">目標外幣代碼</param>
    /// <returns>計算結果</returns>
    /// <exception cref="ArgumentException">金額無效或貨幣不支援時拋出</exception>
    /// <exception cref="InvalidOperationException">無匯率資料時拋出</exception>
    Task<CalculationResult> CalculateTwdToForeignAsync(decimal twdAmount, string targetCurrency);

    /// <summary>
    /// 計算外幣轉台幣金額
    /// </summary>
    /// <param name="foreignAmount">外幣金額</param>
    /// <param name="sourceCurrency">來源外幣代碼</param>
    /// <returns>計算結果</returns>
    /// <exception cref="ArgumentException">金額無效或貨幣不支援時拋出</exception>
    /// <exception cref="InvalidOperationException">無匯率資料時拋出</exception>
    Task<CalculationResult> CalculateForeignToTwdAsync(decimal foreignAmount, string sourceCurrency);

    /// <summary>
    /// 取得當前匯率資料
    /// </summary>
    /// <returns>匯率資料，若無資料則返回 null</returns>
    Task<ExchangeRateData?> GetRatesAsync();

    /// <summary>
    /// 檢查匯率資料是否過期
    /// </summary>
    /// <returns>資料是否過期</returns>
    Task<bool> IsDataStaleAsync();
}
