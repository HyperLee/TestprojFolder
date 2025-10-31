using System.Globalization;

namespace BNICalculate.Helpers;

/// <summary>
/// 日期時間格式化輔助類別
/// </summary>
public static class DateTimeHelper
{
    private static readonly TimeZoneInfo TaiwanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Taipei");
    
    private static readonly Dictionary<DayOfWeek, string> ChineseDayOfWeek = new()
    {
        { DayOfWeek.Monday, "一" },
        { DayOfWeek.Tuesday, "二" },
        { DayOfWeek.Wednesday, "三" },
        { DayOfWeek.Thursday, "四" },
        { DayOfWeek.Friday, "五" },
        { DayOfWeek.Saturday, "六" },
        { DayOfWeek.Sunday, "日" }
    };

    /// <summary>
    /// 取得當前台灣時間（UTC+8）
    /// </summary>
    /// <returns>台灣時間</returns>
    public static DateTime GetTaiwanTime()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TaiwanTimeZone);
    }

    /// <summary>
    /// 格式化日期為中文格式（包含星期）
    /// </summary>
    /// <param name="dateTime">日期時間</param>
    /// <returns>格式化的日期字串，例如："2025年11月1日 (五)"</returns>
    public static string GetChineseDateString(DateTime dateTime)
    {
        var dayOfWeek = ChineseDayOfWeek[dateTime.DayOfWeek];
        return $"{dateTime.Year}年{dateTime.Month}月{dateTime.Day}日 ({dayOfWeek})";
    }

    /// <summary>
    /// 格式化時間為24小時制
    /// </summary>
    /// <param name="dateTime">日期時間</param>
    /// <returns>格式化的時間字串，例如："14:30:00"</returns>
    public static string GetTimeString(DateTime dateTime)
    {
        return dateTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// 格式化完整日期時間（中文日期 + 24小時制時間）
    /// </summary>
    /// <param name="dateTime">日期時間</param>
    /// <returns>格式化的完整日期時間字串，例如："2025年11月1日 (五) 14:30:00"</returns>
    public static string GetFullDateTimeString(DateTime dateTime)
    {
        return $"{GetChineseDateString(dateTime)} {GetTimeString(dateTime)}";
    }
}
