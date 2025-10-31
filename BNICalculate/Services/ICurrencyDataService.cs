using BNICalculate.Models;

namespace BNICalculate.Services;

/// <summary>
/// 匯率資料存取服務介面
/// </summary>
public interface ICurrencyDataService
{
    /// <summary>
    /// 從檔案載入匯率資料
    /// </summary>
    /// <returns>匯率資料，若檔案不存在則返回 null</returns>
    /// <exception cref="DataFormatException">資料格式錯誤時拋出</exception>
    Task<ExchangeRateData?> LoadAsync();

    /// <summary>
    /// 儲存匯率資料至檔案
    /// </summary>
    /// <param name="data">要儲存的匯率資料</param>
    Task SaveAsync(ExchangeRateData data);

    /// <summary>
    /// 檢查匯率資料檔案是否存在
    /// </summary>
    /// <returns>檔案是否存在</returns>
    Task<bool> ExistsAsync();

    /// <summary>
    /// 取得匯率資料檔案的最後修改時間
    /// </summary>
    /// <returns>最後修改時間，若檔案不存在則返回 null</returns>
    Task<DateTime?> GetLastModifiedTimeAsync();
}
