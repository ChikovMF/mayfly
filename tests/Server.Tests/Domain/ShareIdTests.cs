using Server.Domain.ValueObjects;

namespace Server.Tests.Domain;

public class ShareIdTests
{
    [Fact]
    public void New_ReturnsDifferentIds()
    {
        // Act
        var first = ShareId.New();
        var second = ShareId.New();

        // Assert
        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Parse_StringOfExistingId_ReturnsEqualId()
    {
        // Arrange
        var expected = ShareId.New();

        // Act
        var id = ShareId.Parse(expected.ToString());

        // Assert
        Assert.Equal(expected, id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("          ")]
    public void Parse_InvalidString_ThrowsFormatException(string value)
    {
        // Act
        var act = () => ShareId.Parse(value);

        // Assert
        Assert.Throws<FormatException>(act);
    }

    [Fact]
    public void Parse_Null_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ShareId.Parse(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void TryParse_StringOfExistingId_ReturnsTrueAndEqualId()
    {
        // Arrange
        var expected = ShareId.New();

        // Act
        var success = ShareId.TryParse(expected.ToString(), out var id);

        // Assert
        Assert.True(success);
        Assert.Equal(expected, id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("          ")]
    public void TryParse_InvalidString_ReturnsFalseAndNull(string? value)
    {
        // Act
        var success = ShareId.TryParse(value, out var id);

        // Assert
        Assert.False(success);
        Assert.Null(id);
    }

    [Fact]
    public void Equals_SameId_ReturnsTrue()
    {
        // Arrange
        var first = ShareId.New();
        var second = ShareId.Parse(first.ToString());

        // Act
        var result = first.Equals(second);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_DifferentIds_ReturnsFalse()
    {
        // Arrange
        var first = ShareId.New();
        var second = ShareId.New();

        // Act
        var result = first.Equals(second);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ToString_ParsedId_ReturnsOriginalString()
    {
        // Arrange
        var value = ShareId.New().ToString();
        var id = ShareId.Parse(value);

        // Act
        var result = id.ToString();

        // Assert
        Assert.Equal(value, result);
    }
}
