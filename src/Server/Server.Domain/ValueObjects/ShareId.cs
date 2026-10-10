using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace Server.Domain.ValueObjects;

public sealed record ShareId
{
    private const int Length = 10;

    private const string Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
    private static readonly SearchValues<char> AlphabetValues = SearchValues.Create(Alphabet);

    private readonly string _value;

    private ShareId(string value) => _value = value;

    public static ShareId New() => new(RandomNumberGenerator.GetString(Alphabet, Length));

    /// <exception cref="ArgumentNullException"><paramref name="s"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException"><paramref name="s"/> is not a valid share id.</exception>
    public static ShareId Parse(string s)
    {
        ArgumentNullException.ThrowIfNull(s);
        return TryParse(s, out var result)
            ? result
            : throw new FormatException($"'{s}' is not a valid share id.");
    }

    public static bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(false)] out ShareId result)
    {
        if (s is null || s.Length != Length || s.AsSpan().ContainsAnyExcept(AlphabetValues))
        {
            result = null;
            return false;
        }

        result = new ShareId(s);
        return true;
    }

    public override string ToString() => _value;
}