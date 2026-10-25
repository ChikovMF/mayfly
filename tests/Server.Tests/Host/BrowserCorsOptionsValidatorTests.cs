using Server.Host.Cors;

namespace Server.Tests.Host;

public class BrowserCorsOptionsValidatorTests
{
    private readonly BrowserCorsOptionsValidator _validator = new();

    [Fact]
    public void Validate_DefaultOptions_Succeeds()
    {
        // Arrange
        var options = new BrowserCorsOptions();

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData("http://localhost:5173")]
    [InlineData("https://mayfly.example")]
    [InlineData("https://mayfly.example/")]
    [InlineData("https://mayfly.example/app")]
    public void Validate_HttpUrl_Succeeds(string origin)
    {
        // Arrange
        var options = new BrowserCorsOptions { AllowedOrigins = [origin] };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData("mayfly.example")]
    [InlineData("ftp://mayfly.example")]
    [InlineData("*")]
    public void Validate_NotHttpUrl_Fails(string origin)
    {
        // Arrange
        var options = new BrowserCorsOptions { AllowedOrigins = [origin] };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
    }
}
