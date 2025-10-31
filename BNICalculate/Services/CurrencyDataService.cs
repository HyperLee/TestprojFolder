using BNICalculate.Models;
using Microsoft.AspNetCore.Hosting;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace BNICalculate.Services;

/// <summary>
/// 匯率資料存取服務實作
/// </summary>
public class CurrencyDataService : ICurrencyDataService
{
    private readonly string _dataFilePath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 支援中文
    };

    /// <summary>
    /// 建立 CurrencyDataService 實例
    /// </summary>
    /// <param name="env">Web 主機環境</param>
    public CurrencyDataService(IWebHostEnvironment env)
    {
        var dataDirectory = Path.Combine(env.ContentRootPath, "App_Data", "currency");
        Directory.CreateDirectory(dataDirectory); // 確保目錄存在
        _dataFilePath = Path.Combine(dataDirectory, "rates.json");
    }

    /// <summary>
    /// 從檔案載入匯率資料
    /// </summary>
    /// <returns>匯率資料，若檔案不存在則返回 null</returns>
    /// <exception cref="DataFormatException">資料格式錯誤時拋出</exception>
    public async Task<ExchangeRateData?> LoadAsync()
    {
        if (!File.Exists(_dataFilePath))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_dataFilePath);
            var data = JsonSerializer.Deserialize<ExchangeRateData>(json, JsonOptions);
            return data;
        }
        catch (JsonException ex)
        {
            throw new DataFormatException("JSON 格式錯誤", ex);
        }
    }

    /// <summary>
    /// 儲存匯率資料至檔案（使用原子寫入）
    /// </summary>
    /// <param name="data">要儲存的匯率資料</param>
    public async Task SaveAsync(ExchangeRateData data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        
        // 原子寫入：先寫入臨時檔案，再重新命名
        var tempFilePath = _dataFilePath + ".tmp";
        await File.WriteAllTextAsync(tempFilePath, json);
        
        // 重新命名（覆蓋舊檔案）
        File.Move(tempFilePath, _dataFilePath, overwrite: true);
    }

    /// <summary>
    /// 檢查匯率資料檔案是否存在
    /// </summary>
    /// <returns>檔案是否存在</returns>
    public Task<bool> ExistsAsync()
    {
        return Task.FromResult(File.Exists(_dataFilePath));
    }

    /// <summary>
    /// 取得匯率資料檔案的最後修改時間
    /// </summary>
    /// <returns>最後修改時間，若檔案不存在則返回 null</returns>
    public Task<DateTime?> GetLastModifiedTimeAsync()
    {
        if (!File.Exists(_dataFilePath))
        {
            return Task.FromResult<DateTime?>(null);
        }

        var lastModified = File.GetLastWriteTime(_dataFilePath);
        return Task.FromResult<DateTime?>(lastModified);
    }
}
