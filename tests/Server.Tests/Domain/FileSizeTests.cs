using Server.Domain;

namespace Server.Tests.Domain;

public class FileSizeTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(long.MaxValue)]
    public void TryCreate_NonNegativeBytes_ReturnsTrueAndSizeWithGivenBytes(long bytes)
    {
        // Act
        var success = FileSize.TryCreate(bytes, out var size);

        // Assert
        Assert.True(success);
        Assert.NotNull(size);
        Assert.Equal(bytes, size.Bytes);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(long.MinValue)]
    public void TryCreate_NegativeBytes_ReturnsFalseAndNull(long bytes)
    {
        // Act
        var success = FileSize.TryCreate(bytes, out var size);

        // Assert
        Assert.False(success);
        Assert.Null(size);
    }

    [Fact]
    public void Equals_SameBytes_ReturnsTrue()
    {
        // Arrange
        FileSize.TryCreate(100, out var first);
        FileSize.TryCreate(100, out var second);

        // Act
        var result = first!.Equals(second);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_DifferentBytes_ReturnsFalse()
    {
        // Arrange
        FileSize.TryCreate(100, out var first);
        FileSize.TryCreate(200, out var second);

        // Act
        var result = first!.Equals(second);

        // Assert
        Assert.False(result);
    }
}
