using System.Net.Http;

namespace BNICalculate.Tests.Manual;

/// <summary>
/// 手動測試：觸發更新匯率並檢查主控台輸出
/// </summary>
public class ManualUpdateRatesTest
{
    [Fact]
    public async Task TriggerUpdateRates_CheckConsoleOutput()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("Manual Test: Trigger Update Rates");
        Console.WriteLine("========================================");
        Console.WriteLine();
        
        // 等待應用程式啟動
        await Task.Delay(2000);
        
        Console.WriteLine("Sending POST request to update rates...");
        Console.WriteLine();
        
        using var client = new HttpClient();
        client.BaseAddress = new Uri("http://localhost:5087");
        
        try
        {
            // 先載入頁面取得 AntiForgery token (簡化版，只測試 API)
            var response = await client.PostAsync("/CurrencyConverter?handler=UpdateRates", null);
            
            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine($"Response Headers: {response.Headers}");
            
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine();
                Console.WriteLine("✅ Update triggered successfully!");
                Console.WriteLine("Check the application console for detailed logs.");
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine($"⚠️ Response: {response.StatusCode}");
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Content preview: {content.Substring(0, Math.Min(500, content.Length))}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
        
        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine("Now check the DEBUG CONSOLE for:");
        Console.WriteLine("1. API call timing logs");
        Console.WriteLine("2. Chinese character encoding");
        Console.WriteLine("3. No garbled text (亂碼)");
        Console.WriteLine("========================================");
    }
}
