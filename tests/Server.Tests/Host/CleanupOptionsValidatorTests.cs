using Server.Host.Cleanup;

namespace Server.Tests.Host;

public class CleanupOptionsValidatorTests
{
    private readonly CleanupOptionsValidator _validator = new();

    [Fact]
    public void Validate_DefaultOptions_Succeeds()
    {
        // Arrange
        var options = new CleanupOptions();

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData("00:00:00")]
    [InlineData("-00:01:00")]
    [InlineData("60.00:00:00")]
    public void Validate_InvalidInterval_Fails(string interval)
    {
        // Arrange
        var options = new CleanupOptions { Interval = TimeSpan.Parse(interval, System.Globalization.CultureInfo.InvariantCulture) };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
    }

    [Fact]
    public void Validate_ZeroLease_Fails()
    {
        // Arrange
        var options = new CleanupOptions { Lease = TimeSpan.Zero };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
    }

    [Fact]
    public void Validate_NegativeExhaustedFileDelay_Fails()
    {
        // Arrange
        var options = new CleanupOptions { ExhaustedFileDelay = TimeSpan.FromMinutes(-1) };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
    }

    [Fact]
    public void Validate_ZeroExhaustedFileDelay_Succeeds()
    {
        // Arrange
        var options = new CleanupOptions { ExhaustedFileDelay = TimeSpan.Zero };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_ZeroMaxAttempts_Fails()
    {
        // Arrange
        var options = new CleanupOptions { MaxAttempts = 0 };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
    }
}
