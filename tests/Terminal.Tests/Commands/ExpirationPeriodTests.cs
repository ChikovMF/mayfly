using System.CommandLine;
using System.Globalization;
using Mayfly.Terminal.Commands.Upload;
using Mayfly.Terminal.Properties;

namespace Terminal.Tests.Commands;

public class ExpirationPeriodTests
{
    [Theory]
    [InlineData("45s", 45)]
    [InlineData("30m", 30 * 60)]
    [InlineData("12h", 12 * 3600)]
    [InlineData("7d", 7 * 86400)]
    [InlineData("1h", 3600)]
    public void Parse_ValidPeriod_ReturnsPeriod(string value, int expectedSeconds)
    {
        // Arrange
        var option = CreateOption();

        // Act
        var result = new RootCommand { option }.Parse(["--expires-in", value]);

        // Assert
        Assert.Empty(result.Errors);
        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), result.GetValue(option));
    }

    [Theory]
    [InlineData("")]
    [InlineData("h")]
    [InlineData("5")]
    [InlineData("0h")]
    [InlineData("-5m")]
    [InlineData("1.5h")]
    [InlineData("5x")]
    [InlineData("abc")]
    [InlineData("2147483647d")]
    [InlineData("99999999999d")]
    public void Parse_InvalidPeriod_ReportsError(string value)
    {
        // Arrange
        var option = CreateOption();

        // Act
        var result = new RootCommand { option }.Parse(["--expires-in", value]);

        // Assert
        var error = Assert.Single(result.Errors);
        Assert.Equal(string.Format(CultureInfo.CurrentCulture, Resources.InvalidExpiresIn, value), error.Message);
    }

    private static Option<TimeSpan?> CreateOption() =>
        new("--expires-in") { CustomParser = ExpirationPeriod.Parse };
}
