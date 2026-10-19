using Server.Infrastructure.Storage.YandexDisk;

namespace Server.Tests.Infrastructure;

public class YandexDiskOptionsValidatorTests
{
    private readonly YandexDiskOptionsValidator _validator = new();

    [Fact]
    public void Validate_TokenSet_Succeeds()
    {
        // Arrange
        var options = new YandexDiskOptions { OAuthToken = "token" };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_BlankToken_Fails(string token)
    {
        // Arrange
        var options = new YandexDiskOptions { OAuthToken = token };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
    }
}
