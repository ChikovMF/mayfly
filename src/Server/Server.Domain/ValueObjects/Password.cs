using System.Diagnostics.CodeAnalysis;

namespace Server.Domain.ValueObjects;

public sealed record Password
{
    private const int MaxLength = 128;

    private readonly string _value;

    private Password(string value) => _value = value;

    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException"><paramref name="value"/> is not valid.</exception>
    public static Password Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return TryParse(value, out var result)
            ? result
            : throw new FormatException("The value is not a valid password.");
    }

    public static bool TryParse([NotNullWhen(true)] string? value, [MaybeNullWhen(false)] out Password result)
    {
        if (string.IsNullOrEmpty(value) || value.Length > MaxLength)
        {
            result = null;
            return false;
        }

        result = new Password(value);
        return true;
    }

    public override string ToString() => _value;
}
