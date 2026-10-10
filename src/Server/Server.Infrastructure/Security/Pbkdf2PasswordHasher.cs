using System.Globalization;
using System.Security.Cryptography;
using Server.Domain.Abstractions;
using Server.Domain.ValueObjects;

namespace Server.Infrastructure.Security;

internal sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int Iterations = 600_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;

    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string Hash(Password password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password.ToString(), salt, Iterations, Algorithm, KeySize);

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}");
    }

    public bool Verify(Password password, string passwordHash)
    {
        var parts = passwordHash.Split('.');
        var iterations = int.Parse(parts[0], CultureInfo.InvariantCulture);
        var salt = Convert.FromBase64String(parts[1]);
        var expected = Convert.FromBase64String(parts[2]);

        var actual = Rfc2898DeriveBytes.Pbkdf2(password.ToString(), salt, iterations, Algorithm, expected.Length);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
