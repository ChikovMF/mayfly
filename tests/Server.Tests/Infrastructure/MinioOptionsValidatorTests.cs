using Server.Infrastructure.Storage;

namespace Server.Tests.Infrastructure;

public class MinioOptionsValidatorTests
{
    private readonly MinioOptionsValidator _validator = new();

    [Fact]
    public void Validate_AllRequiredSettingsSet_Succeeds()
    {
        // Arrange
        var options = new MinioOptions { Endpoint = "localhost:9000", AccessKey = "key", SecretKey = "secret", BucketName = "bucket" };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_DefaultOptions_Fails()
    {
        // Arrange
        var options = new MinioOptions();

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_BlankEndpoint_Fails(string endpoint)
    {
        // Arrange
        var options = new MinioOptions { Endpoint = endpoint, AccessKey = "key", SecretKey = "secret", BucketName = "bucket" };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_BlankAccessKey_Fails(string accessKey)
    {
        // Arrange
        var options = new MinioOptions { Endpoint = "localhost:9000", AccessKey = accessKey, SecretKey = "secret", BucketName = "bucket" };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_BlankSecretKey_Fails(string secretKey)
    {
        // Arrange
        var options = new MinioOptions { Endpoint = "localhost:9000", AccessKey = "key", SecretKey = secretKey, BucketName = "bucket" };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_BlankBucketName_Fails(string bucketName)
    {
        // Arrange
        var options = new MinioOptions { Endpoint = "localhost:9000", AccessKey = "key", SecretKey = "secret", BucketName = bucketName };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
    }
}
