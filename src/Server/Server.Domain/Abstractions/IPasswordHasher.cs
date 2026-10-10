using Server.Domain.ValueObjects;

namespace Server.Domain.Abstractions;

public interface IPasswordHasher
{
    string Hash(Password password);

    /// <returns><see langword="true"/> if <paramref name="passwordHash"/> was made from <paramref name="password"/>.</returns>
    bool Verify(Password password, string passwordHash);
}
