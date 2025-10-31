using BNICalculate.Models;
using BNICalculate.Helpers;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Text;

namespace BNICalculate.Services;

/// <summary>
/// 匯率計算服務實作
/// </summary>
public class CurrencyService : ICurrencyService
{
    private readonly ICurrencyDataService _dataService;
    private readonly IMemoryCache _cache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CurrencyService> _logger;
    private const string CacheKey = "CurrencyRates:Latest";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    /// <summary>
    /// 建立 CurrencyService 實例
    /// </summary>
    /// <param name="dataService">資料存取服務</param>
    /// <param name="cache">記憶體快取</param>
    /// <param name="httpClientFactory">HTTP 客戶端工廠</param>
    /// <param name="logger">日誌記錄器</param>
    public CurrencyService(
        ICurrencyDataService dataService, 
        IMemoryCache cache,
        IHttpClientFactory httpClientFactory,
        ILogger<CurrencyService> logger)
    {
        _dataService = dataService;
        _cache = cache;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
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

    /// <summary>
    /// 從台銀 API 取得最新匯率並更新本地資料
    /// </summary>
    /// <returns>更新後的匯率資料</returns>
    /// <exception cref="ExternalServiceException">API 呼叫失敗時拋出</exception>
    /// <exception cref="DataFormatException">資料格式錯誤時拋出</exception>
    public async Task<ExchangeRateData> FetchAndUpdateRatesAsync()
    {
        try
        {
            _logger.LogInformation("開始從台銀 API 取得匯率資料");

            // 建立 HTTP 客戶端
            var httpClient = _httpClientFactory.CreateClient("TaiwanBankApi");
            
            // 呼叫台銀 CSV API
            var response = await httpClient.GetAsync("/xrt/flcsv/0/day");
            
            if (!response.IsSuccessStatusCode)
            {
                throw new ExternalServiceException($"台銀 API 呼叫失敗: HTTP {response.StatusCode}");
            }

            // 讀取 CSV 內容（Big5 編碼）
            var bytes = await response.Content.ReadAsByteArrayAsync();
            var encoding = Encoding.GetEncoding("Big5");
            var csvContent = encoding.GetString(bytes);

            // 解析 CSV
            var rates = ParseCsvContent(csvContent);

            // 建立匯率資料物件
            var ratesData = new ExchangeRateData
            {
                Rates = rates,
                LastFetchTime = DateTimeHelper.GetTaiwanTime(),
                DataSource = "台灣銀行"
            };

            // 儲存到檔案
            await _dataService.SaveAsync(ratesData);

            // 清除快取，強制重新載入
            _cache.Remove(CacheKey);

            _logger.LogInformation("成功從台銀 API 取得並儲存 {Count} 筆匯率資料", rates.Count);

            return ratesData;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP 請求失敗");
            throw new ExternalServiceException("無法連線到台銀 API", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "請求逾時");
            throw new ExternalServiceException("台銀 API 請求逾時", ex);
        }
        catch (DataFormatException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得匯率資料時發生未預期的錯誤");
            throw new ExternalServiceException("取得匯率資料失敗", ex);
        }
    }

    /// <summary>
    /// 解析 CSV 內容
    /// </summary>
    private List<ExchangeRate> ParseCsvContent(string csvContent)
    {
        try
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null
            };

            using var reader = new StringReader(csvContent);
            using var csv = new CsvReader(reader, config);
            
            var records = csv.GetRecords<TaiwanBankCsvRecord>().ToList();
            var rates = new List<ExchangeRate>();
            var supportedCurrencies = new HashSet<string> { "USD", "JPY", "CNY", "EUR", "GBP", "HKD", "AUD" };

            foreach (var record in records)
            {
                // 只處理支援的貨幣
                if (!supportedCurrencies.Contains(record.CurrencyCode))
                {
                    continue;
                }

                // 解析匯率（可能包含破折號 "-" 表示無資料）
                if (!decimal.TryParse(record.CashBuyRate, out var buyRate) || buyRate <= 0)
                {
                    _logger.LogWarning("貨幣 {Currency} 的現金買入匯率無效: {Rate}", record.CurrencyCode, record.CashBuyRate);
                    continue;
                }

                if (!decimal.TryParse(record.CashSellRate, out var sellRate) || sellRate <= 0)
                {
                    _logger.LogWarning("貨幣 {Currency} 的現金賣出匯率無效: {Rate}", record.CurrencyCode, record.CashSellRate);
                    continue;
                }

                rates.Add(new ExchangeRate
                {
                    CurrencyCode = record.CurrencyCode,
                    CurrencyName = record.CurrencyName,
                    CashBuyRate = buyRate,
                    CashSellRate = sellRate,
                    LastUpdated = DateTimeHelper.GetTaiwanTime()
                });
            }

            if (rates.Count == 0)
            {
                throw new DataFormatException("CSV 解析後沒有有效的匯率資料");
            }

            return rates;
        }
        catch (Exception ex) when (ex is not DataFormatException)
        {
            _logger.LogError(ex, "CSV 解析失敗");
            throw new DataFormatException("CSV 格式錯誤", ex);
        }
    }
}
