using Server.Domain.ValueObjects;

namespace Server.Tests.Domain;

public class PasswordTests
{
    [Fact]
    public void TryParse_ValidPassword_ReturnsTrueAndPassword()
    {
        // Arrange
        var expected = Password.Parse("secret");

        // Act
        var success = Password.TryParse("secret", out var password);

        // Assert
        Assert.True(success);
        Assert.Equal(expected, password);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void TryParse_NoPassword_ReturnsFalseAndNull(string? value)
    {
        // Act
        var success = Password.TryParse(value, out var password);

        // Assert
        Assert.False(success);
        Assert.Null(password);
    }

    [Fact]
    public void Equals_SamePassword_ReturnsTrue()
    {
        // Arrange
        var first = Password.Parse("secret");
        var second = Password.Parse("secret");

        // Act
        var result = first.Equals(second);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_DifferentPasswords_ReturnsFalse()
    {
        // Arrange
        var first = Password.Parse("secret");
        var second = Password.Parse("another");

        // Act
        var result = first.Equals(second);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ToString_ReturnsPassword()
    {
        // Arrange
        var password = Password.Parse("secret");

        // Act
        var result = password.ToString();

        // Assert
        Assert.Equal("secret", result);
    }

    [Fact]
    public void Parse_ValidString_ReturnsEqualValue()
    {
        // Arrange
        var expected = Password.Parse("secret");

        // Act
        var password = Password.Parse("secret");

        // Assert
        Assert.Equal(expected, password);
    }

    [Fact]
    public void Parse_EmptyString_ThrowsFormatException()
    {
        // Act
        var act = () => Password.Parse("");

        // Assert
        Assert.Throws<FormatException>(act);
    }

    [Fact]
    public void Parse_Null_ThrowsArgumentNullException()
    {
        // Act
        var act = () => Password.Parse(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }
}
