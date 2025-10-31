namespace BNICalculate.Tests.Integration.Pages;

/// <summary>
/// BMI 頁面整合測試
/// </summary>
public class BMIPageTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public BMIPageTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task BMIPage_ReturnsSuccessStatusCode()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/BMI");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task BMIPage_ContainsPageTitle()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/BMI");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("BMI 計算器", content);
    }

    [Fact]
    public async Task BMIPage_ContainsHeightInput()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/BMI");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("id=\"height\"", content);
    }

    [Fact]
    public async Task BMIPage_ContainsWeightInput()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/BMI");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("id=\"weight\"", content);
    }

    [Fact]
    public async Task BMIPage_ContainsCalculateButton()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/BMI");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("id=\"calculate-btn\"", content);
    }

    [Fact]
    public async Task BMIPage_ContainsResultDisplayArea()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/BMI");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("id=\"bmi-value\"", content);
        Assert.Contains("id=\"bmi-category\"", content);
    }

    [Fact]
    public async Task BMIPage_LoadsBMIScript()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/BMI");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("bmi.js", content);
    }

    [Fact]
    public async Task BMIPage_ContainsClearButton()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/BMI");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("id=\"clear-btn\"", content);
    }
}
