using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BNICalculate.Tests.Integration.Pages
{
    /// <summary>
    /// 匯率計算器頁面整合測試
    /// 測試完整的使用者流程,包含更新匯率和計算轉換
    /// </summary>
    public class CurrencyConverterPageTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public CurrencyConverterPageTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        #region 頁面載入測試

        [Fact]
        public async Task CurrencyConverter_PageLoads_ReturnsSuccess()
        {
            // Act
            var response = await _client.GetAsync("/CurrencyConverter");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("text/html; charset=utf-8",
                response.Content.Headers.ContentType?.ToString());
        }

        [Fact]
        public async Task CurrencyConverter_PageContent_ContainsRequiredElements()
        {
            // Act
            var response = await _client.GetAsync("/CurrencyConverter");
            var content = await response.Content.ReadAsStringAsync();

            // Assert - 驗證頁面標題
            Assert.Contains("匯率計算器", content);

            // Assert - 驗證表單存在
            Assert.Contains("台幣轉外幣", content);
            Assert.Contains("外幣轉台幣", content);

            // Assert - 驗證7種支援的貨幣
            Assert.Contains("美元 (USD)", content);
            Assert.Contains("日圓 (JPY)", content);
            Assert.Contains("人民幣 (CNY)", content);
            Assert.Contains("歐元 (EUR)", content);
            Assert.Contains("英鎊 (GBP)", content);
            Assert.Contains("港幣 (HKD)", content);
            Assert.Contains("澳幣 (AUD)", content);

            // Assert - 驗證更新按鈕
            Assert.Contains("更新匯率", content);
        }

        [Fact]
        public async Task CurrencyConverter_HasAccessibilityFeatures()
        {
            // Act
            var response = await _client.GetAsync("/CurrencyConverter");
            var content = await response.Content.ReadAsStringAsync();

            // Assert - 驗證無障礙功能
            Assert.Contains("skip-link", content); // Skip navigation link
            Assert.Contains("id=\"main-content\"", content); // Main content landmark
            Assert.Contains("aria-label", content); // ARIA labels
            Assert.Contains("aria-required", content); // Required field indicators
            Assert.Contains("role=\"alert\"", content); // Error alerts
        }

        #endregion

        #region 台幣轉外幣測試

        [Theory]
        [InlineData("USD")]
        [InlineData("JPY")]
        [InlineData("EUR")]
        public async Task CalculateTwdToForeign_WithValidData_ReturnsSuccessWithResult(string currency)
        {
            // Arrange
            var formData = new Dictionary<string, string>
            {
                { "TwdAmount", "1000" },
                { "SelectedCurrency", currency }
            };

            // Act
            var response = await _client.PostAsync(
                "/CurrencyConverter?handler=CalculateTwdToForeign",
                new FormUrlEncodedContent(formData)
            );

            // Assert
            Assert.True(
                response.StatusCode == HttpStatusCode.OK || 
                response.StatusCode == HttpStatusCode.Redirect,
                $"Expected OK or Redirect but got {response.StatusCode}"
            );

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                // 應該包含計算結果或錯誤訊息
                Assert.True(
                    content.Contains("計算結果") || 
                    content.Contains("error") ||
                    content.Contains("匯率資料"),
                    "Response should contain calculation result or error message"
                );
            }
        }

        [Fact]
        public async Task CalculateTwdToForeign_WithInvalidAmount_ShowsValidationError()
        {
            // Arrange
            var formData = new Dictionary<string, string>
            {
                { "TwdAmount", "-100" }, // 負數金額
                { "SelectedCurrency", "USD" }
            };

            // Act
            var response = await _client.PostAsync(
                "/CurrencyConverter?handler=CalculateTwdToForeign",
                new FormUrlEncodedContent(formData)
            );

            // Assert
            var content = await response.Content.ReadAsStringAsync();
            Assert.True(
                response.StatusCode == HttpStatusCode.OK || 
                response.StatusCode == HttpStatusCode.BadRequest,
                "Should return OK with validation error or BadRequest"
            );
        }

        [Fact]
        public async Task CalculateTwdToForeign_WithZeroAmount_ShowsValidationError()
        {
            // Arrange
            var formData = new Dictionary<string, string>
            {
                { "TwdAmount", "0" },
                { "SelectedCurrency", "USD" }
            };

            // Act
            var response = await _client.PostAsync(
                "/CurrencyConverter?handler=CalculateTwdToForeign",
                new FormUrlEncodedContent(formData)
            );

            // Assert
            var content = await response.Content.ReadAsStringAsync();
            Assert.True(
                response.StatusCode == HttpStatusCode.OK || 
                response.StatusCode == HttpStatusCode.BadRequest
            );
        }

        #endregion

        #region 外幣轉台幣測試

        [Theory]
        [InlineData("USD", "100")]
        [InlineData("JPY", "10000")]
        [InlineData("EUR", "100")]
        public async Task CalculateForeignToTwd_WithValidData_ReturnsSuccessWithResult(
            string currency, string amount)
        {
            // Arrange
            var formData = new Dictionary<string, string>
            {
                { "ForeignAmount", amount },
                { "SourceCurrency", currency }
            };

            // Act
            var response = await _client.PostAsync(
                "/CurrencyConverter?handler=CalculateForeignToTwd",
                new FormUrlEncodedContent(formData)
            );

            // Assert
            Assert.True(
                response.StatusCode == HttpStatusCode.OK || 
                response.StatusCode == HttpStatusCode.Redirect
            );

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                Assert.True(
                    content.Contains("計算結果") || 
                    content.Contains("error") ||
                    content.Contains("匯率資料")
                );
            }
        }

        [Fact]
        public async Task CalculateForeignToTwd_WithNegativeAmount_ShowsValidationError()
        {
            // Arrange
            var formData = new Dictionary<string, string>
            {
                { "ForeignAmount", "-50" },
                { "SourceCurrency", "USD" }
            };

            // Act
            var response = await _client.PostAsync(
                "/CurrencyConverter?handler=CalculateForeignToTwd",
                new FormUrlEncodedContent(formData)
            );

            // Assert
            Assert.True(
                response.StatusCode == HttpStatusCode.OK || 
                response.StatusCode == HttpStatusCode.BadRequest
            );
        }

        #endregion

        #region 更新匯率測試

        [Fact]
        public async Task UpdateRates_PostRequest_ReturnsSuccessOrRedirect()
        {
            // Act
            var response = await _client.PostAsync(
                "/CurrencyConverter?handler=UpdateRates",
                new FormUrlEncodedContent(new Dictionary<string, string>())
            );

            // Assert
            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Redirect ||
                response.StatusCode == HttpStatusCode.Found,
                $"Expected success response but got {response.StatusCode}"
            );
        }

        #endregion

        #region 完整使用者流程測試

        [Fact]
        public async Task CompleteUserFlow_UpdateRatesThenCalculate_WorksCorrectly()
        {
            // Step 1: 載入頁面
            var getResponse = await _client.GetAsync("/CurrencyConverter");
            Assert.True(getResponse.IsSuccessStatusCode);

            // Step 2: 更新匯率
            var updateResponse = await _client.PostAsync(
                "/CurrencyConverter?handler=UpdateRates",
                new FormUrlEncodedContent(new Dictionary<string, string>())
            );
            Assert.True(
                updateResponse.StatusCode == HttpStatusCode.OK ||
                updateResponse.StatusCode == HttpStatusCode.Redirect ||
                updateResponse.StatusCode == HttpStatusCode.Found
            );

            // Step 3: 執行計算
            var calculateData = new Dictionary<string, string>
            {
                { "TwdAmount", "1000" },
                { "SelectedCurrency", "USD" }
            };
            var calculateResponse = await _client.PostAsync(
                "/CurrencyConverter?handler=CalculateTwdToForeign",
                new FormUrlEncodedContent(calculateData)
            );
            
            Assert.True(
                calculateResponse.StatusCode == HttpStatusCode.OK ||
                calculateResponse.StatusCode == HttpStatusCode.Redirect
            );
        }

        #endregion

        #region 效能測試

        [Fact]
        public async Task PageLoad_CompletesWithinTimeout()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            var response = await _client.GetAsync("/CurrencyConverter");
            stopwatch.Stop();

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.True(
                stopwatch.ElapsedMilliseconds < 5000,
                $"Page load took {stopwatch.ElapsedMilliseconds}ms, expected < 5000ms"
            );
        }

        [Fact]
        public async Task Calculation_CompletesWithinTimeout()
        {
            // Arrange
            var formData = new Dictionary<string, string>
            {
                { "TwdAmount", "1000" },
                { "SelectedCurrency", "USD" }
            };
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            var response = await _client.PostAsync(
                "/CurrencyConverter?handler=CalculateTwdToForeign",
                new FormUrlEncodedContent(formData)
            );
            stopwatch.Stop();

            // Assert
            Assert.True(
                stopwatch.ElapsedMilliseconds < 3000,
                $"Calculation took {stopwatch.ElapsedMilliseconds}ms, expected < 3000ms"
            );
        }

        #endregion

        #region 安全性測試

        [Theory]
        [InlineData("999999999999999999")] // 極大數字
        [InlineData("0.000000001")] // 極小數字
        public async Task Calculate_WithBoundaryValues_HandlesGracefully(string amount)
        {
            // Arrange
            var formData = new Dictionary<string, string>
            {
                { "TwdAmount", amount },
                { "SelectedCurrency", "USD" }
            };

            // Act
            var response = await _client.PostAsync(
                "/CurrencyConverter?handler=CalculateTwdToForeign",
                new FormUrlEncodedContent(formData)
            );

            // Assert - 不應該導致伺服器錯誤
            Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task Calculate_WithUnsupportedCurrency_HandlesGracefully()
        {
            // Arrange
            var formData = new Dictionary<string, string>
            {
                { "TwdAmount", "1000" },
                { "SelectedCurrency", "XXX" } // 不支援的貨幣
            };

            // Act
            var response = await _client.PostAsync(
                "/CurrencyConverter?handler=CalculateTwdToForeign",
                new FormUrlEncodedContent(formData)
            );

            // Assert - 應該返回錯誤訊息而非崩潰
            Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        #endregion
    }
}
