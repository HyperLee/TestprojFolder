using System.Text.Json;

using Microsoft.Extensions.Caching.Memory;

namespace BNICalculate.Services;

/// <summary>
/// 番茄鐘資料服務 - 負責 JSON 檔案讀寫、快取管理
/// </summary>
public class PomodoroDataService
{
    private readonly string _dataPath;
    private readonly IMemoryCache _cache;
    private readonly JsonSerializerOptions _jsonOptions;
    private const string SettingsCacheKey = "PomodoroSettings";

    public PomodoroDataService(IWebHostEnvironment env, IMemoryCache cache)
    {
        _dataPath = Path.Combine(env.ContentRootPath, "App_Data", "pomodoro");
        _cache = cache;

        // 確保資料目錄存在
        Directory.CreateDirectory(_dataPath);

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// 載入使用者設定（使用快取）
    /// </summary>
    public async Task<Models.UserSettings> LoadSettingsAsync()
    {
        // 嘗試從快取取得
        if (_cache.TryGetValue(SettingsCacheKey, out Models.UserSettings? cachedSettings) && cachedSettings != null)
        {
            return cachedSettings;
        }

        // 從檔案讀取
        var filePath = Path.Combine(_dataPath, "settings.json");
        Models.UserSettings settings;

        try
        {
            if (!File.Exists(filePath))
            {
                settings = Models.UserSettings.Default();
            }
            else
            {
                var json = await File.ReadAllTextAsync(filePath);
                settings = JsonSerializer.Deserialize<Models.UserSettings>(json, _jsonOptions)
                           ?? Models.UserSettings.Default();
            }
        }
        catch (Exception)
        {
            // 發生錯誤時恢復預設值
            settings = Models.UserSettings.Default();
        }

        // 快取 10 分鐘滑動過期
        _cache.Set(SettingsCacheKey, settings, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(10)
        });

        return settings;
    }

    /// <summary>
    /// 儲存使用者設定
    /// </summary>
    public async Task SaveSettingsAsync(Models.UserSettings settings)
    {
        settings.LastModifiedUtc = DateTime.UtcNow;

        var filePath = Path.Combine(_dataPath, "settings.json");
        var json = JsonSerializer.Serialize(settings, _jsonOptions);

        try
        {
            await File.WriteAllTextAsync(filePath, json);

            // 更新快取
            _cache.Set(SettingsCacheKey, settings, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(10)
            });
        }
        catch (Exception)
        {
            // 儲存失敗，記錄錯誤但不拋出例外
            // 實際應用中應加入日誌記錄
            throw;
        }
    }

    /// <summary>
    /// 載入今日統計（不快取，確保即時性）
    /// </summary>
    public async Task<Models.PomodoroStatistics> LoadTodayStatsAsync()
    {
        var filePath = Path.Combine(_dataPath, "stats.json");

        try
        {
            if (!File.Exists(filePath))
            {
                return Models.PomodoroStatistics.CreateForToday();
            }

            var json = await File.ReadAllTextAsync(filePath);
            var stats = JsonSerializer.Deserialize<Models.PomodoroStatistics>(json, _jsonOptions);

            if (stats == null || !stats.IsToday())
            {
                // 跨日重置
                return Models.PomodoroStatistics.CreateForToday();
            }

            return stats;
        }
        catch (Exception)
        {
            // 發生錯誤時恢復預設值
            return Models.PomodoroStatistics.CreateForToday();
        }
    }

    /// <summary>
    /// 儲存統計資料
    /// </summary>
    public async Task SaveStatsAsync(Models.PomodoroStatistics stats)
    {
        stats.LastUpdatedUtc = DateTime.UtcNow;

        var filePath = Path.Combine(_dataPath, "stats.json");
        var json = JsonSerializer.Serialize(stats, _jsonOptions);

        try
        {
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception)
        {
            // 儲存失敗，記錄錯誤但不拋出例外
            throw;
        }
    }

    /// <summary>
    /// 記錄完成的工作時段
    /// </summary>
    public async Task RecordCompletedSessionAsync(Models.TimerSession session)
    {
        var stats = await LoadTodayStatsAsync();

        if (session.SessionType == "work")
        {
            stats.RecordWorkSession(session);
        }
        else if (session.SessionType == "break")
        {
            stats.RecordBreakSession(session);
        }

        await SaveStatsAsync(stats);
    }
}