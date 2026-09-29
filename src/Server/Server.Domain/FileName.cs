using System.Diagnostics.CodeAnalysis;

namespace Server.Domain;

public sealed record FileName
{
    private const int MaxLength = 255;

    private readonly string _value;

    private FileName(string value) => _value = value;

    public static bool TryCreate([NotNullWhen(true)] string? value, [MaybeNullWhen(false)] out FileName result)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed) ||
            trimmed.Length > MaxLength ||
            trimmed is "." or ".." ||
            trimmed.AsSpan().ContainsAny('/', '\\') ||
            trimmed.Any(char.IsControl))
        {
            result = null;
            return false;
        }

        result = new FileName(trimmed);
        return true;
    }

    public override string ToString() => _value;
}
