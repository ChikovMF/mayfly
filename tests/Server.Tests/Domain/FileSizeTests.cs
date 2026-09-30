using Server.Domain;

namespace Server.Tests.Domain;

public class FileSizeTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(long.MaxValue)]
    public void TryParse_NonNegativeBytes_ReturnsTrueAndSizeWithGivenBytes(long bytes)
    {
        // Act
        var success = FileSize.TryParse(bytes, out var size);

        // Assert
        Assert.True(success);
        Assert.NotNull(size);
        Assert.Equal(bytes, size.Bytes);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(long.MinValue)]
    public void TryParse_NegativeBytes_ReturnsFalseAndNull(long bytes)
    {
        // Act
        var success = FileSize.TryParse(bytes, out var size);

        // Assert
        Assert.False(success);
        Assert.Null(size);
    }

    [Fact]
    public void Equals_SameBytes_ReturnsTrue()
    {
        // Arrange
        var first = FileSize.Parse(100);
        var second = FileSize.Parse(100);

        // Act
        var result = first.Equals(second);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_DifferentBytes_ReturnsFalse()
    {
        // Arrange
        var first = FileSize.Parse(100);
        var second = FileSize.Parse(200);

        // Act
        var result = first.Equals(second);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Parse_NonNegativeBytes_ReturnsSizeWithGivenBytes()
    {
        // Act
        var size = FileSize.Parse(1024);

        // Assert
        Assert.Equal(1024, size.Bytes);
    }

    [Fact]
    public void Parse_NegativeBytes_ThrowsFormatException()
    {
        // Act
        var act = () => FileSize.Parse(-1);

        // Assert
        Assert.Throws<FormatException>(act);
    }
}
