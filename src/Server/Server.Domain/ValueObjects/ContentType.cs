using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

namespace Server.Domain.ValueObjects;

public sealed record ContentType
{
    private const string Wildcard = "*";

    private readonly string _value;

    private ContentType(string value) => _value = value;

    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException"><paramref name="value"/> is not valid.</exception>
    public static ContentType Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return TryParse(value, out var result)
            ? result
            : throw new FormatException($"'{value}' is not a valid content type.");
    }

    public static bool TryParse([NotNullWhen(true)] string? value, [MaybeNullWhen(false)] out ContentType result)
    {
        if (!MediaTypeHeaderValue.TryParse(value, out var parsed) ||
            parsed.MediaType is null ||
            parsed.MediaType.Split('/').Contains(Wildcard))
        {
            result = null;
            return false;
        }

        var normalized = new MediaTypeHeaderValue(parsed.MediaType.ToLowerInvariant());
        foreach (var parameter in parsed.Parameters)
        {
            normalized.Parameters.Add(new NameValueHeaderValue(parameter.Name.ToLowerInvariant(), parameter.Value));
        }

        result = new ContentType(normalized.ToString());
        return true;
    }

    public override string ToString() => _value;
}
