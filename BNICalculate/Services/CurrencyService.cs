using BNICalculate.Models;
using Microsoft.Extensions.Caching.Memory;

namespace BNICalculate.Services;

/// <summary>
/// 匯率計算服務實作
/// </summary>
public class CurrencyService : ICurrencyService
{
    private readonly ICurrencyDataService _dataService;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "CurrencyRates:Latest";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    /// <summary>
    /// 建立 CurrencyService 實例
    /// </summary>
    /// <param name="dataService">資料存取服務</param>
    /// <param name="cache">記憶體快取</param>
    public CurrencyService(ICurrencyDataService dataService, IMemoryCache cache)
    {
        _dataService = dataService;
        _cache = cache;
    }

    /// <summary>
    /// 計算台幣轉外幣金額
    /// </summary>
    /// <param name="twdAmount">台幣金額</param>
    /// <param name="targetCurrency">目標外幣代碼</param>
    /// <returns>計算結果</returns>
    /// <exception cref="ArgumentException">金額無效或貨幣不支援時拋出</exception>
    /// <exception cref="InvalidOperationException">無匯率資料時拋出</exception>
    public async Task<CalculationResult> CalculateTwdToForeignAsync(decimal twdAmount, string targetCurrency)
    {
        // 驗證金額
        if (twdAmount <= 0)
        {
            throw new ArgumentException("金額必須為正數", nameof(twdAmount));
        }

        // 取得匯率資料
        var ratesData = await GetRatesAsync();
        if (ratesData == null)
        {
            throw new InvalidOperationException("無匯率資料");
        }

        // 取得目標貨幣匯率
        var rate = ratesData.GetRate(targetCurrency);
        if (rate == null)
        {
            throw new ArgumentException($"不支援的貨幣代碼: {targetCurrency}", nameof(targetCurrency));
        }

        // 計算：使用現金賣出匯率（銀行賣出外幣給客戶）
        // 台幣金額 / 賣出匯率 = 外幣金額
        var exchangeRate = 1 / rate.CashSellRate;
        var foreignAmount = twdAmount * exchangeRate;
        var roundedAmount = Math.Round(foreignAmount, 6);

        return new CalculationResult
        {
            SourceAmount = twdAmount,
            SourceCurrency = "TWD",
            TargetAmount = roundedAmount,
            TargetCurrency = targetCurrency,
            ExchangeRate = Math.Round(exchangeRate, 6),
            CalculatedAt = DateTime.Now
        };
    }

    /// <summary>
    /// 計算外幣轉台幣金額
    /// </summary>
    /// <param name="foreignAmount">外幣金額</param>
    /// <param name="sourceCurrency">來源外幣代碼</param>
    /// <returns>計算結果</returns>
    /// <exception cref="ArgumentException">金額無效或貨幣不支援時拋出</exception>
    /// <exception cref="InvalidOperationException">無匯率資料時拋出</exception>
    public async Task<CalculationResult> CalculateForeignToTwdAsync(decimal foreignAmount, string sourceCurrency)
    {
        // 驗證金額
        if (foreignAmount <= 0)
        {
            throw new ArgumentException("金額必須為正數", nameof(foreignAmount));
        }

        // 取得匯率資料
        var ratesData = await GetRatesAsync();
        if (ratesData == null)
        {
            throw new InvalidOperationException("無匯率資料");
        }

        // 取得來源貨幣匯率
        var rate = ratesData.GetRate(sourceCurrency);
        if (rate == null)
        {
            throw new ArgumentException($"不支援的貨幣代碼: {sourceCurrency}", nameof(sourceCurrency));
        }

        // 計算：使用現金買入匯率（銀行買入外幣）
        // 外幣金額 * 買入匯率 = 台幣金額
        var twdAmount = foreignAmount * rate.CashBuyRate;
        var roundedAmount = Math.Round(twdAmount, 6);

        return new CalculationResult
        {
            SourceAmount = foreignAmount,
            SourceCurrency = sourceCurrency,
            TargetAmount = roundedAmount,
            TargetCurrency = "TWD",
            ExchangeRate = rate.CashBuyRate,
            CalculatedAt = DateTime.Now
        };
    }

    /// <summary>
    /// 取得當前匯率資料
    /// </summary>
    /// <returns>匯率資料，若無資料則返回 null</returns>
    public async Task<ExchangeRateData?> GetRatesAsync()
    {
        // 先檢查快取
        if (_cache.TryGetValue(CacheKey, out ExchangeRateData? cachedData))
        {
            return cachedData;
        }

        // 從檔案載入
        var data = await _dataService.LoadAsync();
        if (data != null)
        {
            // 儲存到快取（30分鐘滑動過期）
            _cache.Set(CacheKey, data, new MemoryCacheEntryOptions
            {
                SlidingExpiration = CacheDuration,
                Priority = CacheItemPriority.Normal
            });
        }

        return data;
    }

    /// <summary>
    /// 檢查匯率資料是否過期
    /// </summary>
    /// <returns>資料是否過期</returns>
    public async Task<bool> IsDataStaleAsync()
    {
        var data = await GetRatesAsync();
        if (data == null)
        {
            return true; // 無資料視為過期
        }

        return data.IsStale();
    }
}
