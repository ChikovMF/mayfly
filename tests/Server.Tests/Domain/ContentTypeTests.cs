using Server.Domain;

namespace Server.Tests.Domain;

public class ContentTypeTests
{
    [Fact]
    public void TryParse_ValidMediaType_ReturnsTrueAndContentType()
    {
        // Arrange
        ContentType.TryParse("text/plain", out var expected);

        // Act
        var success = ContentType.TryParse("text/plain", out var contentType);

        // Assert
        Assert.True(success);
        Assert.Equal(expected, contentType);
    }

    [Fact]
    public void TryParse_MediaTypeInDifferentCase_ReturnsContentTypeEqualToLowercase()
    {
        // Arrange
        ContentType.TryParse("text/plain", out var expected);

        // Act
        var success = ContentType.TryParse("Text/PLAIN", out var contentType);

        // Assert
        Assert.True(success);
        Assert.Equal(expected, contentType);
    }

    [Fact]
    public void TryParse_ParameterNameInDifferentCase_ReturnsContentTypeEqualToLowercase()
    {
        // Arrange
        ContentType.TryParse("text/plain; charset=utf-8", out var expected);

        // Act
        var success = ContentType.TryParse("text/plain; CHARSET=utf-8", out var contentType);

        // Assert
        Assert.True(success);
        Assert.Equal(expected, contentType);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("          ")]
    [InlineData("plain")]
    [InlineData("*/*")]
    [InlineData("text/*")]
    [InlineData("*/plain")]
    public void TryParse_InvalidMediaType_ReturnsFalseAndNull(string? value)
    {
        // Act
        var success = ContentType.TryParse(value, out var contentType);

        // Assert
        Assert.False(success);
        Assert.Null(contentType);
    }

    [Fact]
    public void Equals_SameMediaType_ReturnsTrue()
    {
        // Arrange
        ContentType.TryParse("text/plain", out var first);
        ContentType.TryParse("text/plain", out var second);

        // Act
        var result = first!.Equals(second);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_DifferentMediaTypes_ReturnsFalse()
    {
        // Arrange
        ContentType.TryParse("text/plain", out var first);
        ContentType.TryParse("image/png", out var second);

        // Act
        var result = first!.Equals(second);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ToString_ReturnsMediaType()
    {
        // Arrange
        ContentType.TryParse("image/png", out var contentType);

        // Act
        var result = contentType!.ToString();

        // Assert
        Assert.Equal("image/png", result);
    }

    [Fact]
    public void ToString_MediaTypeInDifferentCase_ReturnsLowercase()
    {
        // Arrange
        ContentType.TryParse("Image/PNG", out var contentType);

        // Act
        var result = contentType!.ToString();

        // Assert
        Assert.Equal("image/png", result);
    }
}
