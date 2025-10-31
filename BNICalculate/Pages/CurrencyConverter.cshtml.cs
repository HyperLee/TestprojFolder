using BNICalculate.Models;
using BNICalculate.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BNICalculate.Pages;

/// <summary>
/// 匯率計算器頁面模型
/// </summary>
public class CurrencyConverterModel : PageModel
{
    private readonly ICurrencyService _currencyService;
    private readonly ILogger<CurrencyConverterModel> _logger;

    public CurrencyConverterModel(
        ICurrencyService currencyService,
        ILogger<CurrencyConverterModel> logger)
    {
        _currencyService = currencyService;
        _logger = logger;
    }

    [BindProperty]
    public decimal TwdAmount { get; set; }

    [BindProperty]
    public string SelectedCurrency { get; set; } = "USD";

    [BindProperty]
    public decimal ForeignAmount { get; set; }

    [BindProperty]
    public string SourceCurrency { get; set; } = "USD";

    public CalculationResult? Result { get; set; }

    public ExchangeRateData? CurrentRates { get; set; }

    public bool IsDataStale { get; set; }

    public string? ErrorMessage { get; set; }

    public bool IsUpdating { get; set; }

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
