using BNICalculate.Models;
using System.ComponentModel.DataAnnotations;

namespace BNICalculate.Tests.Unit.Models;

/// <summary>
/// CalculationRequest 模型測試
/// </summary>
public class CalculationRequestTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Amount_Should_BePositive(decimal invalidAmount)
    {
        // Arrange
        var request = new CalculationRequest
        {
            Amount = invalidAmount,
            CurrencyCode = "USD"
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(request);
        var isValid = Validator.TryValidateObject(request, context, validationResults, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, v => 
            v.MemberNames.Contains(nameof(CalculationRequest.Amount)) && 
            v.ErrorMessage!.Contains("必須為正數"));
    }

    [Fact]
    public void CurrencyCode_Should_BeRequired()
    {
        // Arrange
        var request = new CalculationRequest
        {
            Amount = 1000,
            CurrencyCode = ""
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(request);
        var isValid = Validator.TryValidateObject(request, context, validationResults, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(CalculationRequest.CurrencyCode)));
    }

    [Fact]
    public void IsValid_Should_ReturnTrue_ForValidRequest()
    {
        // Arrange
        var request = new CalculationRequest
        {
            Amount = 1000,
            CurrencyCode = "USD"
        };

        // Act
        var isValid = request.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Theory]
    [InlineData(0, "USD")]
    [InlineData(-1, "USD")]
    [InlineData(1000, "")]
    [InlineData(1000, null)]
    public void IsValid_Should_ReturnFalse_ForInvalidRequest(decimal amount, string? currencyCode)
    {
        // Arrange
        var request = new CalculationRequest
        {
            Amount = amount,
            CurrencyCode = currencyCode ?? ""
        };

        // Act
        var isValid = request.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ValidCalculationRequest_Should_PassValidation()
    {
        // Arrange
        var request = new CalculationRequest
        {
            Amount = 1000,
            CurrencyCode = "USD"
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(request);
        var isValid = Validator.TryValidateObject(request, context, validationResults, true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(validationResults);
    }
}
