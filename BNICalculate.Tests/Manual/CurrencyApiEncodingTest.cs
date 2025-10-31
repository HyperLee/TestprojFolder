using System.Diagnostics;
using System.Text;

namespace BNICalculate.Tests.Manual;

/// <summary>
/// 手動測試：驗證台銀 API 編碼和效能
/// </summary>
public class CurrencyApiEncodingTest
{
    public CurrencyApiEncodingTest()
    {
        // 註冊 Big5 編碼提供者
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [Fact]
    public async Task TestRealApiCall_CheckEncodingAndPerformance()
    {
        // 直接呼叫台銀 API 並檢查編碼
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://rate.bot.com.tw");
        httpClient.Timeout = TimeSpan.FromSeconds(15);

        Console.WriteLine("========================================");
        Console.WriteLine("Start calling Taiwan Bank API...");
        Console.WriteLine("========================================");
        
        var stopwatch = Stopwatch.StartNew();
        var response = await httpClient.GetAsync("/xrt/flcsv/0/day");
        stopwatch.Stop();

        Console.WriteLine($"HTTP request time: {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine($"HTTP status: {response.StatusCode}");

        Assert.True(response.IsSuccessStatusCode, "API call failed");
        Assert.True(stopwatch.ElapsedMilliseconds < 15000, 
            $"API call took {stopwatch.ElapsedMilliseconds} ms, exceeds 15 second limit");

        // Read raw bytes
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Console.WriteLine($"Response size: {bytes.Length} bytes");
        Console.WriteLine();

        // Test Big5 decoding
        Console.WriteLine("========================================");
        Console.WriteLine("Big5 Decoding Test");
        Console.WriteLine("========================================");
        var big5Encoding = Encoding.GetEncoding("Big5");
        var big5Content = big5Encoding.GetString(bytes);
        
        // Verify content contains Chinese characters (Unicode range 4E00-9FFF)
        var chineseCharCount = big5Content.Count(c => c >= 0x4E00 && c <= 0x9FFF);
        Console.WriteLine($"Chinese character count: {chineseCharCount}");
        Assert.True(chineseCharCount > 50, $"Big5 decoding should contain many Chinese characters, actual: {chineseCharCount}");
        
        // Verify no replacement characters (decoding failure marker)
        var replacementCharCount = big5Content.Count(c => c == '\uFFFD');
        Console.WriteLine($"Replacement character count (decoding failure): {replacementCharCount}");
        Assert.True(replacementCharCount == 0, $"Big5 decoding should not contain replacement characters, actual: {replacementCharCount}");
        
        // Verify first line is header
        var firstLine = big5Content.Split('\n')[0];
        var firstLineHasChinese = firstLine.Any(c => c >= 0x4E00 && c <= 0x9FFF);
        Console.WriteLine($"First line has Chinese: {firstLineHasChinese}");
        Assert.True(firstLineHasChinese, "First line should contain Chinese characters");
        
        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine("Encoding test PASSED!");
        Console.WriteLine("========================================");
    }

    [Fact]
    public async Task TestMultipleApiCalls_CheckConsistency()
    {
        // Test multiple API calls for performance consistency
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://rate.bot.com.tw");
        httpClient.Timeout = TimeSpan.FromSeconds(15);

        Console.WriteLine("========================================");
        Console.WriteLine("Testing 3 API calls performance");
        Console.WriteLine("========================================");

        var times = new List<long>();

        for (int i = 1; i <= 3; i++)
        {
            Console.WriteLine($"\nCall {i}:");
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var response = await httpClient.GetAsync("/xrt/flcsv/0/day");
                stopwatch.Stop();
                
                Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds} ms");
                Console.WriteLine($"  Status: {response.StatusCode}");
                
                times.Add(stopwatch.ElapsedMilliseconds);
                
                Assert.True(response.IsSuccessStatusCode);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.WriteLine($"  Failed: {ex.Message}");
                Console.WriteLine($"  Time: {stopwatch.ElapsedMilliseconds} ms");
                throw;
            }

            // Wait 1 second before next call
            if (i < 3)
            {
                await Task.Delay(1000);
            }
        }

        Console.WriteLine("\n========================================");
        Console.WriteLine("Performance Statistics");
        Console.WriteLine("========================================");
        Console.WriteLine($"Average: {times.Average():F2} ms");
        Console.WriteLine($"Fastest: {times.Min()} ms");
        Console.WriteLine($"Slowest: {times.Max()} ms");
        
        var allUnder5Seconds = times.All(t => t < 5000);
        Console.WriteLine($"\nAll calls under 5 seconds: {(allUnder5Seconds ? "YES" : "NO")}");
        
        Assert.True(times.All(t => t < 15000), "Some calls exceeded 15 seconds");
    }
}
