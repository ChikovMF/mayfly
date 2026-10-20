using Server.Domain.ValueObjects;

namespace Server.Tests.Domain;

public class DownloadsLeftTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void TryParse_NonNegativeCount_ReturnsTrueAndDownloadsLeftWithGivenCount(int count)
    {
        // Act
        var success = DownloadsLeft.TryParse(count, out var downloadsLeft);

        // Assert
        Assert.True(success);
        Assert.NotNull(downloadsLeft);
        Assert.Equal(count, downloadsLeft.Count);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void TryParse_NegativeCount_ReturnsFalseAndNull(int count)
    {
        // Act
        var success = DownloadsLeft.TryParse(count, out var downloadsLeft);

        // Assert
        Assert.False(success);
        Assert.Null(downloadsLeft);
    }

    [Fact]
    public void Equals_SameCount_ReturnsTrue()
    {
        // Arrange
        var first = DownloadsLeft.Parse(3);
        var second = DownloadsLeft.Parse(3);

        // Act
        var result = first.Equals(second);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_DifferentCounts_ReturnsFalse()
    {
        // Arrange
        var first = DownloadsLeft.Parse(3);
        var second = DownloadsLeft.Parse(4);

        // Act
        var result = first.Equals(second);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Parse_NonNegativeCount_ReturnsDownloadsLeftWithGivenCount()
    {
        // Act
        var downloadsLeft = DownloadsLeft.Parse(5);

        // Assert
        Assert.Equal(5, downloadsLeft.Count);
    }

    [Fact]
    public void Parse_NegativeCount_ThrowsFormatException()
    {
        // Act
        var act = () => DownloadsLeft.Parse(-1);

        // Assert
        Assert.Throws<FormatException>(act);
    }
}
