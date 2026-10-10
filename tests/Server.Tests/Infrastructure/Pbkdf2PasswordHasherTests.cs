using Server.Domain.ValueObjects;
using Server.Infrastructure.Security;

namespace Server.Tests.Infrastructure;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void Hash_SamePasswordTwice_ReturnsDifferentHashes()
    {
        // Arrange
        var password = Password.Parse("secret");

        // Act
        var first = _hasher.Hash(password);
        var second = _hasher.Hash(password);

        // Assert
        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Verify_PasswordTheHashWasMadeFrom_ReturnsTrue()
    {
        // Arrange
        var hash = _hasher.Hash(Password.Parse("secret"));

        // Act
        var result = _hasher.Verify(Password.Parse("secret"), hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Verify_AnotherPassword_ReturnsFalse()
    {
        // Arrange
        var hash = _hasher.Hash(Password.Parse("secret"));

        // Act
        var result = _hasher.Verify(Password.Parse("another"), hash);

        // Assert
        Assert.False(result);
    }
}
