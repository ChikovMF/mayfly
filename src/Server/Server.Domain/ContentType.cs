using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

namespace Server.Domain;

public sealed record ContentType
{
    private const string Wildcard = "*";

    private readonly string _value;

    private ContentType(string value) => _value = value;

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
