using BNICalculate.Models;
using BNICalculate.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

    /// <summary>
    /// T055: 記錄完成的時段 API
    /// </summary>
    public async Task<IActionResult> OnPostRecordCompleteAsync([FromBody] RecordSessionRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.SessionType) ||
                (request.SessionType != "work" && request.SessionType != "break"))
            {
                return BadRequest(new { success = false, message = "無效的時段類型" });
            }

            // 建立時段記錄
            TimerSession session;
            if (request.SessionType == "work")
            {
                session = TimerSession.CreateWorkSession(request.DurationMinutes);
            }
            else
            {
                session = TimerSession.CreateBreakSession(request.DurationMinutes);
            }

            // 記錄至統計
            await _dataService.RecordCompletedSessionAsync(session);

            // 重新載入統計以取得最新計數
            var stats = await _dataService.LoadTodayStatsAsync();

            return new JsonResult(new
            {
                success = true,
                completedPomodoroCount = stats.CompletedPomodoroCount,
                completedBreakCount = stats.CompletedBreakCount
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"記錄失敗：{ex.Message}" });
        }
    }

    /// <summary>
    /// T076: 儲存使用者設定 Handler
    /// </summary>
    public async Task<IActionResult> OnPostSaveSettingsAsync([FromBody] UserSettings settings)
    {
        try
        {
            // T077: ModelState 驗證（Range 檢查）
            if (!ModelState.IsValid || !settings.IsValid())
            {
                return BadRequest(new { success = false, message = "設定值無效。工作時長：1-60 分鐘，休息時長：1-30 分鐘" });
            }

            // T078: 儲存至 JSON、更新 IMemoryCache
            await _dataService.SaveSettingsAsync(settings);

            return new JsonResult(new { success = true, message = "設定已儲存" });
        }
        catch (Exception ex)
        {
            // T079: 錯誤處理
            return StatusCode(500, new { success = false, message = $"儲存失敗：{ex.Message}" });
        }
    }

    /// <summary>
    /// 記錄時段請求模型
    /// </summary>
    public class RecordSessionRequest
    {
        public string SessionType { get; set; } = "work";
        public int DurationMinutes { get; set; }
    }
}