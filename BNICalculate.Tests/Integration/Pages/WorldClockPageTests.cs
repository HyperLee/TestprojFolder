using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BNICalculate.Tests.Integration.Pages
{
    /// <summary>
    /// 世界時鐘頁面整合測試
    /// </summary>
    public class WorldClockPageTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public WorldClockPageTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task WorldClock_ReturnsSuccessAndCorrectContentType()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/WorldClock");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("text/html; charset=utf-8", 
                response.Content.Headers.ContentType?.ToString());
        }

        [Fact]
        public async Task WorldClock_DisplaysTenCities()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/WorldClock");
            var content = await response.Content.ReadAsStringAsync();

            // Assert - 驗證 10 個城市名稱都出現在頁面中
            Assert.Contains("台北", content);
            Assert.Contains("東京", content);
            Assert.Contains("倫敦", content);
            Assert.Contains("紐約", content);
            Assert.Contains("洛杉磯", content);
            Assert.Contains("巴黎", content);
            Assert.Contains("柏林", content);
            Assert.Contains("莫斯科", content);
            Assert.Contains("新加坡", content);
            Assert.Contains("悉尼", content);
        }

        [Fact]
        public async Task WorldClock_HasCorrectHtmlStructure()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/WorldClock");
            var content = await response.Content.ReadAsStringAsync();

            // Assert - 驗證 HTML 結構
            Assert.Contains("id=\"main-clock\"", content); // 主要時鐘區域
            Assert.Contains("id=\"city-grid\"", content);  // 城市網格容器
            Assert.Contains("id=\"loading\"", content);    // 載入指示器
            Assert.Contains("id=\"error-message\"", content); // 錯誤訊息容器
            
            // 驗證日期格式為 YYYY-MM-DD（透過 id 檢查）
            Assert.Contains("id=\"main-date\"", content);
        }
    }
}
