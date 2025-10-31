namespace BNICalculate.Models;

/// <summary>
/// 番茄鐘每日統計資料
/// </summary>
public class PomodoroStatistics
{
    /// <summary>
    /// 統計日期（yyyy-MM-dd 格式）
    /// </summary>
    public string Date { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");

    /// <summary>
    /// 完成的番茄鐘數量（僅計算工作時段）
    /// </summary>
    public int CompletedPomodoroCount { get; set; } = 0;

    /// <summary>
    /// 完成的休息時段數量
    /// </summary>
    public int CompletedBreakCount { get; set; } = 0;

    /// <summary>
    /// 總工作時長（分鐘）
    /// </summary>
    public double TotalWorkMinutes { get; set; } = 0;

    /// <summary>
    /// 總休息時長（分鐘）
    /// </summary>
    public double TotalBreakMinutes { get; set; } = 0;

    /// <summary>
    /// 時段明細列表（可選，用於歷史追蹤）
    /// </summary>
    public List<TimerSession> Sessions { get; set; } = new List<TimerSession>();

    /// <summary>
    /// 最後更新時間（UTC）
    /// </summary>
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 建立今日統計
    /// </summary>
    public static PomodoroStatistics CreateForToday()
    {
        return new PomodoroStatistics
        {
            Date = DateTime.Today.ToString("yyyy-MM-dd"),
            CompletedPomodoroCount = 0,
            CompletedBreakCount = 0,
            TotalWorkMinutes = 0,
            TotalBreakMinutes = 0,
            Sessions = new List<TimerSession>(),
            LastUpdatedUtc = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 檢查是否為今日統計
    /// </summary>
    public bool IsToday()
    {
        return Date == DateTime.Today.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// 記錄完成的工作時段
    /// </summary>
    public void RecordWorkSession(TimerSession session)
    {
        if (session.SessionType != "work") return;

        CompletedPomodoroCount++;
        TotalWorkMinutes += session.ActualDurationMinutes;
        Sessions.Add(session);
        LastUpdatedUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// 記錄完成的休息時段
    /// </summary>
    public void RecordBreakSession(TimerSession session)
    {
        if (session.SessionType != "break") return;

        CompletedBreakCount++;
        TotalBreakMinutes += session.ActualDurationMinutes;
        Sessions.Add(session);
        LastUpdatedUtc = DateTime.UtcNow;
    }
}