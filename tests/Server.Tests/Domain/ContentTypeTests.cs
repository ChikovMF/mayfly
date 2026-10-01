using Server.Domain.ValueObjects;

namespace Server.Tests.Domain;

public class ContentTypeTests
{
    [Fact]
    public void TryParse_ValidMediaType_ReturnsTrueAndContentType()
    {
        // Arrange
        var expected = ContentType.Parse("text/plain");

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
        var expected = ContentType.Parse("text/plain");

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
        var expected = ContentType.Parse("text/plain; charset=utf-8");

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
        var first = ContentType.Parse("text/plain");
        var second = ContentType.Parse("text/plain");

        // Act
        var result = first.Equals(second);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_DifferentMediaTypes_ReturnsFalse()
    {
        // Arrange
        var first = ContentType.Parse("text/plain");
        var second = ContentType.Parse("image/png");

        // Act
        var result = first.Equals(second);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ToString_ReturnsMediaType()
    {
        // Arrange
        var contentType = ContentType.Parse("image/png");

        // Act
        var result = contentType.ToString();

        // Assert
        Assert.Equal("image/png", result);
    }

    [Fact]
    public void ToString_MediaTypeInDifferentCase_ReturnsLowercase()
    {
        // Arrange
        var contentType = ContentType.Parse("Image/PNG");

        // Act
        var result = contentType.ToString();

        // Assert
        Assert.Equal("image/png", result);
    }

    [Fact]
    public void Parse_ValidString_ReturnsEqualValue()
    {
        // Arrange
        var expected = ContentType.Parse("text/plain");

        // Act
        var contentType = ContentType.Parse("text/plain");

        // Assert
        Assert.Equal(expected, contentType);
    }

    [Theory]
    [InlineData("")]
    [InlineData("          ")]
    [InlineData("*/*")]
    public void Parse_InvalidString_ThrowsFormatException(string value)
    {
        // Act
        var act = () => ContentType.Parse(value);

        // Assert
        Assert.Throws<FormatException>(act);
    }

    [Fact]
    public void Parse_Null_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ContentType.Parse(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }
}
