using Server.Host.Files;

namespace Server.Tests.Host;

public class RetentionOptionsValidatorTests
{
    private readonly RetentionOptionsValidator _validator = new();

    [Fact]
    public void Validate_DefaultOptions_Succeeds()
    {
        // Arrange
        var options = new RetentionOptions();

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_PositiveMaxLifetime_Succeeds()
    {
        // Arrange
        var options = new RetentionOptions { MaxLifetime = TimeSpan.FromDays(7) };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData("00:00:00")]
    [InlineData("-00:01:00")]
    public void Validate_NotPositiveMaxLifetime_Fails(string maxLifetime)
    {
        // Arrange
        var options = new RetentionOptions { MaxLifetime = TimeSpan.Parse(maxLifetime, System.Globalization.CultureInfo.InvariantCulture) };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
    }
}
