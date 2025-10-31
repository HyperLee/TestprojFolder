using Microsoft.AspNetCore.Mvc.RazorPages;
using BNICalculate.Services;
using BNICalculate.Models;

namespace BNICalculate.Pages;

/// <summary>
/// 番茄鐘計時器頁面模型
/// </summary>
public class PomodoroModel : PageModel
{
    private readonly PomodoroDataService _dataService;
    
    /// <summary>
    /// 使用者設定
    /// </summary>
    public UserSettings Settings { get; set; } = UserSettings.Default();
    
    /// <summary>
    /// 今日統計
    /// </summary>
    public PomodoroStatistics Statistics { get; set; } = PomodoroStatistics.CreateForToday();
    
    public PomodoroModel(PomodoroDataService dataService)
    {
        _dataService = dataService;
    }
    
    /// <summary>
    /// 頁面載入時執行
    /// </summary>
    public async Task OnGetAsync()
    {
        // 載入使用者設定（使用快取）
        Settings = await _dataService.LoadSettingsAsync();
        
        // 載入今日統計（不快取）
        Statistics = await _dataService.LoadTodayStatsAsync();
    }
}
