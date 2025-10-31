using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using BNICalculate.Services;

namespace BNICalculate.Tests.Manual;

/// <summary>
/// 檢查實際執行時的主控台輸出和編碼
/// 執行這個測試並檢查 DEBUG CONSOLE 的輸出
/// </summary>
public class ConsoleEncodingVerificationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ConsoleEncodingVerificationTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task VerifyConsoleOutput_NoGarbledText()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("Console Encoding Verification Test");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine("CHECK THE DEBUG CONSOLE (Output window) FOR:");
        Console.WriteLine("1. [INF] messages with Chinese characters");
        Console.WriteLine("2. API timing logs");
        Console.WriteLine("3. Verify NO garbled text (question marks or strange symbols)");
        Console.WriteLine();
        
        // 從 DI 容器取得服務
        using var scope = _factory.Services.CreateScope();
        var currencyService = scope.ServiceProvider.GetRequiredService<ICurrencyService>();
        
        Console.WriteLine("Calling FetchAndUpdateRatesAsync...");
        Console.WriteLine("(This will appear in DEBUG CONSOLE with detailed logs)");
        Console.WriteLine();
        
        try
        {
            // 執行更新 - 這會在 DEBUG CONSOLE 輸出詳細日誌
            var result = await currencyService.FetchAndUpdateRatesAsync();
            
            Console.WriteLine("========================================");
            Console.WriteLine("Update completed! Result:");
            Console.WriteLine("========================================");
            Console.WriteLine($"Data Source: {result.DataSource}");
            Console.WriteLine($"Last Fetch: {result.LastFetchTime:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Rates Count: {result.Rates.Count}");
            Console.WriteLine();
            
            Console.WriteLine("Currency Names (checking encoding):");
            Console.WriteLine("========================================");
            
            foreach (var rate in result.Rates)
            {
                // 計算中文字元
                var chineseCount = rate.CurrencyName.Count(c => c >= 0x4E00 && c <= 0x9FFF);
                var hasReplacement = rate.CurrencyName.Contains('\uFFFD');
                var status = hasReplacement ? "FAIL" : (chineseCount > 0 ? "OK" : "WARN");
                
                Console.WriteLine($"[{status}] {rate.CurrencyCode}: {rate.CurrencyName} (Chinese chars: {chineseCount})");
                
                if (hasReplacement)
                {
                    Console.WriteLine($"      ^ ERROR: Contains replacement character (garbled!)");
                }
            }
            
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("Summary:");
            Console.WriteLine("========================================");
            
            var totalChinese = result.Rates.Sum(r => r.CurrencyName.Count(c => c >= 0x4E00 && c <= 0x9FFF));
            var totalReplacement = result.Rates.Sum(r => r.CurrencyName.Count(c => c == '\uFFFD'));
            
            Console.WriteLine($"Total Chinese characters: {totalChinese}");
            Console.WriteLine($"Total garbled characters: {totalReplacement}");
            Console.WriteLine($"Result: {(totalReplacement == 0 && totalChinese > 0 ? "PASS" : "FAIL")}");
            
            // 斷言
            Assert.True(totalChinese > 0, $"Expected Chinese characters, found {totalChinese}");
            Assert.Equal(0, totalReplacement);
            
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("TEST PASSED!");
            Console.WriteLine("========================================");
            Console.WriteLine("Now check the DEBUG CONSOLE for detailed logs");
            Console.WriteLine("Look for lines starting with [INF]");
            Console.WriteLine("Verify Chinese text displays correctly");
            Console.WriteLine("========================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine($"TEST FAILED: {ex.Message}");
            Console.WriteLine("========================================");
            Console.WriteLine($"Exception Type: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }
}
