using System.Security.Cryptography;
using Server.Domain.ValueObjects;

namespace Server.Tests.Domain;

public class Sha256Tests
{
    [Fact]
    public void TryParse_Sha256Digest_ReturnsTrueAndHashWithLowercaseHex()
    {
        // Arrange
        var digest = SHA256.HashData("hello"u8);

        // Act
        var success = Sha256.TryParse(digest, out var sha256);

        // Assert
        Assert.True(success);
        Assert.NotNull(sha256);
        Assert.Equal(Convert.ToHexStringLower(digest), sha256.Hex);
    }

    [Fact]
    public void TryParse_Empty_ReturnsFalseAndNull()
    {
        // Act
        var success = Sha256.TryParse([], out var sha256);

        // Assert
        Assert.False(success);
        Assert.Null(sha256);
    }

    [Fact]
    public void ToBytes_ReturnsParsedBytes()
    {
        // Arrange
        var digest = SHA256.HashData("hello"u8);
        var sha256 = Sha256.Parse(digest);

        // Act
        var bytes = sha256.ToBytes();

        // Assert
        Assert.Equal(digest, bytes);
    }

    [Fact]
    public void Equals_SameBytes_ReturnsTrue()
    {
        // Arrange
        var first = Sha256.Parse(SHA256.HashData("hello"u8));
        var second = Sha256.Parse(SHA256.HashData("hello"u8));

        // Act
        var equal = first.Equals(second);

        // Assert
        Assert.True(equal);
    }

    [Fact]
    public void Equals_DifferentBytes_ReturnsFalse()
    {
        // Arrange
        var first = Sha256.Parse(SHA256.HashData("hello"u8));
        var second = Sha256.Parse(SHA256.HashData("world"u8));

        // Act
        var equal = first.Equals(second);

        // Assert
        Assert.False(equal);
    }
}
