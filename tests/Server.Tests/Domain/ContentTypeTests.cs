using Server.Domain;

namespace Server.Tests.Domain;

public class ContentTypeTests
{
    [Fact]
    public void TryCreate_ValidMediaType_ReturnsTrueAndContentType()
    {
        // Arrange
        ContentType.TryCreate("text/plain", out var expected);

        // Act
        var success = ContentType.TryCreate("text/plain", out var contentType);

        // Assert
        Assert.True(success);
        Assert.Equal(expected, contentType);
    }

    [Fact]
    public void TryCreate_MediaTypeInDifferentCase_ReturnsContentTypeEqualToLowercase()
    {
        // Arrange
        ContentType.TryCreate("text/plain", out var expected);

        // Act
        var success = ContentType.TryCreate("Text/PLAIN", out var contentType);

        // Assert
        Assert.True(success);
        Assert.Equal(expected, contentType);
    }

    [Fact]
    public void TryCreate_ParameterNameInDifferentCase_ReturnsContentTypeEqualToLowercase()
    {
        // Arrange
        ContentType.TryCreate("text/plain; charset=utf-8", out var expected);

        // Act
        var success = ContentType.TryCreate("text/plain; CHARSET=utf-8", out var contentType);

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
    public void TryCreate_InvalidMediaType_ReturnsFalseAndNull(string? value)
    {
        // Act
        var success = ContentType.TryCreate(value, out var contentType);

        // Assert
        Assert.False(success);
        Assert.Null(contentType);
    }

    [Fact]
    public void Equals_SameMediaType_ReturnsTrue()
    {
        // Arrange
        ContentType.TryCreate("text/plain", out var first);
        ContentType.TryCreate("text/plain", out var second);

        // Act
        var result = first!.Equals(second);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_DifferentMediaTypes_ReturnsFalse()
    {
        // Arrange
        ContentType.TryCreate("text/plain", out var first);
        ContentType.TryCreate("image/png", out var second);

        // Act
        var result = first!.Equals(second);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ToString_ReturnsMediaType()
    {
        // Arrange
        ContentType.TryCreate("image/png", out var contentType);

        // Act
        var result = contentType!.ToString();

        // Assert
        Assert.Equal("image/png", result);
    }

    [Fact]
    public void ToString_MediaTypeInDifferentCase_ReturnsLowercase()
    {
        // Arrange
        ContentType.TryCreate("Image/PNG", out var contentType);

        // Act
        var result = contentType!.ToString();

        // Assert
        Assert.Equal("image/png", result);
    }
}
