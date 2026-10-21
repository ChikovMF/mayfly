using Server.Infrastructure.Storage.S3;

namespace Server.Tests.Infrastructure;

public class S3OptionsValidatorTests
{
    private readonly S3OptionsValidator _validator = new();

    [Fact]
    public void Validate_AllRequiredSettingsSet_Succeeds()
    {
        // Arrange
        var options = new S3Options { ServiceUrl = "http://localhost:9000", AccessKey = "key", SecretKey = "secret", BucketName = "bucket" };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_DefaultOptions_Fails()
    {
        // Arrange
        var options = new S3Options();

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
    }

    [Theory]
    [InlineData("")]
    [InlineData("localhost:9000")]
    [InlineData("/minio")]
    [InlineData("ftp://localhost:9000")]
    public void Validate_InvalidServiceUrl_Fails(string serviceUrl)
    {
        // Arrange
        var options = new S3Options { ServiceUrl = serviceUrl, AccessKey = "key", SecretKey = "secret", BucketName = "bucket" };

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
        var options = new S3Options { ServiceUrl = "http://localhost:9000", AccessKey = accessKey, SecretKey = "secret", BucketName = "bucket" };

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
        var options = new S3Options { ServiceUrl = "http://localhost:9000", AccessKey = "key", SecretKey = secretKey, BucketName = "bucket" };

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
        var options = new S3Options { ServiceUrl = "http://localhost:9000", AccessKey = "key", SecretKey = "secret", BucketName = bucketName };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_BlankRegion_Fails(string region)
    {
        // Arrange
        var options = new S3Options { ServiceUrl = "http://localhost:9000", AccessKey = "key", SecretKey = "secret", BucketName = "bucket", Region = region };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
    }
}
