using Server.Domain;

namespace Server.Tests.Domain;

public class FileNameTests
{
    [Fact]
    public void TryParse_ValidName_ReturnsTrueAndName()
    {
        // Arrange
        FileName.TryParse("report.pdf", out var expected);

        // Act
        var success = FileName.TryParse("report.pdf", out var name);

        // Assert
        Assert.True(success);
        Assert.Equal(expected, name);
    }

    [Fact]
    public void TryParse_NameWithSurroundingWhitespace_ReturnsTrueAndTrimmedName()
    {
        // Arrange
        FileName.TryParse("report.pdf", out var expected);

        // Act
        var success = FileName.TryParse("  report.pdf\t ", out var name);

        // Assert
        Assert.True(success);
        Assert.Equal(expected, name);
    }

    [Fact]
    public void TryParse_NameOfMaxLength_ReturnsTrue()
    {
        // Arrange
        var value = new string('a', 255);

        // Act
        var success = FileName.TryParse(value, out var name);

        // Assert
        Assert.True(success);
        Assert.NotNull(name);
    }

    [Fact]
    public void TryParse_NameLongerThanMaxLength_ReturnsFalseAndNull()
    {
        // Arrange
        var value = new string('a', 256);

        // Act
        var success = FileName.TryParse(value, out var name);

        // Assert
        Assert.False(success);
        Assert.Null(name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("          ")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("dir/file.txt")]
    [InlineData("dir\\file.txt")]
    [InlineData("fi\nle.txt")]
    [InlineData("fi\0le.txt")]
    public void TryParse_InvalidName_ReturnsFalseAndNull(string? value)
    {
        // Act
        var success = FileName.TryParse(value, out var name);

        // Assert
        Assert.False(success);
        Assert.Null(name);
    }

    [Fact]
    public void Equals_SameName_ReturnsTrue()
    {
        // Arrange
        FileName.TryParse("report.pdf", out var first);
        FileName.TryParse("report.pdf", out var second);

        // Act
        var result = first!.Equals(second);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_DifferentNames_ReturnsFalse()
    {
        // Arrange
        FileName.TryParse("report.pdf", out var first);
        FileName.TryParse("photo.png", out var second);

        // Act
        var result = first!.Equals(second);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ToString_ReturnsName()
    {
        // Arrange
        FileName.TryParse("report.pdf", out var name);

        // Act
        var result = name!.ToString();

        // Assert
        Assert.Equal("report.pdf", result);
    }
}
