namespace BNICalculate.Models;

/// <summary>
/// 單次計時器時段記錄
/// </summary>
public class TimerSession
{
    /// <summary>
    /// 唯一識別碼（GUID）
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 時段類型：work（工作）或 break（休息）
    /// </summary>
    public string SessionType { get; set; } = "work";

    /// <summary>
    /// 開始時間（UTC）
    /// </summary>
    public DateTime StartTimeUtc { get; set; }

    /// <summary>
    /// 結束時間（UTC）
    /// </summary>
    public DateTime EndTimeUtc { get; set; }

    /// <summary>
    /// 計畫時長（分鐘）
    /// </summary>
    public int PlannedDurationMinutes { get; set; }

    /// <summary>
    /// 實際時長（分鐘，計算屬性）
    /// </summary>
    public double ActualDurationMinutes => (EndTimeUtc - StartTimeUtc).TotalMinutes;

    /// <summary>
    /// 是否完整完成（未中途放棄）
    /// </summary>
    public bool IsCompleted { get; set; } = true;

    /// <summary>
    /// 記錄日期（本地時區，用於跨日界判斷）
    /// </summary>
    public string RecordDate { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");

    /// <summary>
    /// 建立工作時段
    /// </summary>
    public static TimerSession CreateWorkSession(int durationMinutes)
    {
        var now = DateTime.UtcNow;
        return new TimerSession
        {
            SessionType = "work",
            StartTimeUtc = now,
            EndTimeUtc = now.AddMinutes(durationMinutes),
            PlannedDurationMinutes = durationMinutes,
            IsCompleted = true
        };
    }

    /// <summary>
    /// 建立休息時段
    /// </summary>
    public static TimerSession CreateBreakSession(int durationMinutes)
    {
        var now = DateTime.UtcNow;
        return new TimerSession
        {
            SessionType = "break",
            StartTimeUtc = now,
            EndTimeUtc = now.AddMinutes(durationMinutes),
            PlannedDurationMinutes = durationMinutes,
            IsCompleted = true
        };
    }
}