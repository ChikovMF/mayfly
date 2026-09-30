using Server.Domain;

namespace Server.Tests.Domain;

public class FileNameTests
{
    [Fact]
    public void TryParse_ValidName_ReturnsTrueAndName()
    {
        // Arrange
        var expected = FileName.Parse("report.pdf");

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
        var expected = FileName.Parse("report.pdf");

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
        var first = FileName.Parse("report.pdf");
        var second = FileName.Parse("report.pdf");

        // Act
        var result = first.Equals(second);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_DifferentNames_ReturnsFalse()
    {
        // Arrange
        var first = FileName.Parse("report.pdf");
        var second = FileName.Parse("photo.png");

        // Act
        var result = first.Equals(second);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ToString_ReturnsName()
    {
        // Arrange
        var name = FileName.Parse("report.pdf");

        // Act
        var result = name.ToString();

        // Assert
        Assert.Equal("report.pdf", result);
    }

    [Fact]
    public void Parse_ValidString_ReturnsEqualValue()
    {
        // Arrange
        var expected = FileName.Parse("report.pdf");

        // Act
        var fileName = FileName.Parse("report.pdf");

        // Assert
        Assert.Equal(expected, fileName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("          ")]
    [InlineData("..")]
    public void Parse_InvalidString_ThrowsFormatException(string value)
    {
        // Act
        var act = () => FileName.Parse(value);

        // Assert
        Assert.Throws<FormatException>(act);
    }

    [Fact]
    public void Parse_Null_ThrowsArgumentNullException()
    {
        // Act
        var act = () => FileName.Parse(null);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }
}
