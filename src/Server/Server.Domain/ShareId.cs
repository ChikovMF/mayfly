using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace Server.Domain;

public sealed record ShareId : IParsable<ShareId>
{
    private const int MinLength = 8;
    private const int MaxLength = 16;
    private const int DefaultLength = 10;

    private const string Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
    private static readonly SearchValues<char> AlphabetValues = SearchValues.Create(Alphabet);

    private readonly string _value;

    private ShareId(string value) => _value = value;

    public static ShareId New() => new(RandomNumberGenerator.GetString(Alphabet, DefaultLength));

    public static ShareId Parse(string s) => Parse(s, null);

    public static ShareId Parse(string s, IFormatProvider? provider)
    {
        ArgumentNullException.ThrowIfNull(s);
        return TryParse(s, provider, out var result)
            ? result
            : throw new FormatException($"'{s}' is not a valid share id.");
    }

    public static bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(false)] out ShareId result) =>
        TryParse(s, null, out result);

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out ShareId result)
    {
        if (s is null || s.Length is < MinLength or > MaxLength || s.AsSpan().ContainsAnyExcept(AlphabetValues))
        {
            result = null;
            return false;
        }

        result = new ShareId(s);
        return true;
    }

    public override string ToString() => _value;
}