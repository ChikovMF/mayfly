using System.Diagnostics.CodeAnalysis;

namespace Server.Domain;

public sealed record FileName
{
    private readonly string _value;

    private FileName(string value) => _value = value;

    public static bool TryCreate([NotNullWhen(true)] string? value, [MaybeNullWhen(false)] out FileName result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = null;
            return false;
        }

        result = new FileName(value);
        return true;
    }

    public override string ToString() => _value;
}
