using BNICalculate.Models;
using BNICalculate.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BNICalculate.Pages;

/// <summary>
/// 匯率計算器頁面模型
/// 提供台幣與外幣雙向轉換功能,支援 7 種主要貨幣
/// </summary>
public class CurrencyConverterModel : PageModel
{
    private readonly ICurrencyService _currencyService;
    private readonly ILogger<CurrencyConverterModel> _logger;

    /// <summary>
    /// 初始化匯率計算器頁面模型
    /// </summary>
    /// <param name="currencyService">匯率計算服務</param>
    /// <param name="logger">日誌記錄器</param>
    public CurrencyConverterModel(
        ICurrencyService currencyService,
        ILogger<CurrencyConverterModel> logger)
    {
        _currencyService = currencyService;
        _logger = logger;
    }

    /// <summary>
    /// 台幣金額 (用於台幣轉外幣)
    /// </summary>
    [BindProperty]
    public decimal TwdAmount { get; set; }

    /// <summary>
    /// 選擇的目標外幣代碼
    /// </summary>
    [BindProperty]
    public string SelectedCurrency { get; set; } = "USD";

    /// <summary>
    /// 外幣金額 (用於外幣轉台幣)
    /// </summary>
    [BindProperty]
    public decimal ForeignAmount { get; set; }

    /// <summary>
    /// 選擇的來源外幣代碼
    /// </summary>
    [BindProperty]
    public string SourceCurrency { get; set; } = "USD";

    /// <summary>
    /// 計算結果
    /// </summary>
    public CalculationResult? Result { get; set; }

    /// <summary>
    /// 當前匯率資料
    /// </summary>
    public ExchangeRateData? CurrentRates { get; set; }

    /// <summary>
    /// 匯率資料是否過期 (超過 24 小時)
    /// </summary>
    public bool IsDataStale { get; set; }

    /// <summary>
    /// 錯誤訊息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 是否正在更新匯率
    /// </summary>
    public bool IsUpdating { get; set; }

    /// <summary>
    /// 頁面載入時執行
    /// 自動載入匯率資料,若無資料則從 API 取得
    /// </summary>
    public async Task OnGetAsync()
    {
        try
        {
            CurrentRates = await _currencyService.GetRatesAsync();
            
            // 如果無資料，自動取得
            if (CurrentRates == null)
            {
                _logger.LogInformation("首次載入，自動從台銀 API 取得匯率資料");
                try
                {
                    CurrentRates = await _currencyService.FetchAndUpdateRatesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "自動更新匯率失敗");
                    ErrorMessage = "無法取得匯率資料，請稍後手動更新。";
                }
            }
            
            IsDataStale = await _currencyService.IsDataStaleAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "載入匯率資料時發生錯誤");
            ErrorMessage = "載入匯率資料時發生錯誤，請稍後再試。";
        }
    }

    /// <summary>
    /// 計算台幣轉外幣
    /// </summary>
    /// <returns>頁面結果,包含計算結果或驗證錯誤</returns>
    public async Task<IActionResult> OnPostCalculateTwdToForeignAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        try
        {
            Result = await _currencyService.CalculateTwdToForeignAsync(TwdAmount, SelectedCurrency);
            await OnGetAsync(); // 重新載入匯率資訊
            return Page();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "計算時參數錯誤: {Message}", ex.Message);
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "計算時操作錯誤: {Message}", ex.Message);
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "計算時發生未預期的錯誤");
            ModelState.AddModelError(string.Empty, "計算時發生錯誤，請稍後再試。");
        }

        await OnGetAsync();
        return Page();
    }

    /// <summary>
    /// 計算外幣轉台幣
    /// </summary>
    /// <returns>頁面結果,包含計算結果或驗證錯誤</returns>
    public async Task<IActionResult> OnPostCalculateForeignToTwdAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        try
        {
            Result = await _currencyService.CalculateForeignToTwdAsync(ForeignAmount, SourceCurrency);
            await OnGetAsync(); // 重新載入匯率資訊
            return Page();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "計算時參數錯誤: {Message}", ex.Message);
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "計算時操作錯誤: {Message}", ex.Message);
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "計算時發生未預期的錯誤");
            ModelState.AddModelError(string.Empty, "計算時發生錯誤，請稍後再試。");
        }

        await OnGetAsync();
        return Page();
    }

    /// <summary>
    /// 手動更新匯率資料
    /// 從臺灣銀行 API 取得最新匯率並更新快取
    /// </summary>
    /// <returns>重導向到頁面或顯示錯誤訊息</returns>
    public async Task<IActionResult> OnPostUpdateRatesAsync()
    {
        try
        {
            IsUpdating = true;
            _logger.LogInformation("手動更新匯率");
            
            CurrentRates = await _currencyService.FetchAndUpdateRatesAsync();
            IsDataStale = false;
            
            TempData["SuccessMessage"] = "匯率已成功更新！";
            return RedirectToPage();
        }
        catch (ExternalServiceException ex)
        {
            _logger.LogError(ex, "更新匯率失敗: {Message}", ex.Message);
            ErrorMessage = $"更新失敗：{ex.Message}";
        }
        catch (DataFormatException ex)
        {
            _logger.LogError(ex, "資料格式錯誤: {Message}", ex.Message);
            ErrorMessage = $"資料格式錯誤：{ex.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新匯率時發生未預期的錯誤");
            ErrorMessage = "更新匯率時發生錯誤，請稍後再試。";
        }

        await OnGetAsync();
        return Page();
    }
}
