using System.ComponentModel.DataAnnotations;

namespace BNICalculate.Models;

/// <summary>
/// 番茄工作法使用者設定
/// </summary>
public class UserSettings
{
    /// <summary>
    /// 工作時段時長（分鐘）
    /// </summary>
    [Range(1, 60, ErrorMessage = "工作時長必須在 1-60 分鐘之間")]
    public int WorkDurationMinutes { get; set; } = 25;
    
    /// <summary>
    /// 休息時段時長（分鐘）
    /// </summary>
    [Range(1, 30, ErrorMessage = "休息時長必須在 1-30 分鐘之間")]
    public int BreakDurationMinutes { get; set; } = 5;
    
    /// <summary>
    /// 是否在時段完成時播放音效（未來擴充）
    /// </summary>
    public bool EnableSound { get; set; } = false;
    
    /// <summary>
    /// 最後修改時間（UTC）
    /// </summary>
    public DateTime LastModifiedUtc { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 建立預設設定
    /// </summary>
    public static UserSettings Default() => new UserSettings();
    
    /// <summary>
    /// 驗證設定有效性
    /// </summary>
    public bool IsValid()
    {
        return WorkDurationMinutes >= 1 && WorkDurationMinutes <= 60
            && BreakDurationMinutes >= 1 && BreakDurationMinutes <= 30;
    }
}
