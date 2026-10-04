using System.CommandLine.Parsing;
using Mayfly.Terminal.Properties;

namespace Mayfly.Terminal.Commands;

internal static class ServerAddress
{
    public static Uri? Parse(ArgumentResult result) => Parse(result, result.Tokens.Single().Value);

    public static Uri? Parse(ArgumentResult result, string value)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            return uri;
        }

        result.AddError(string.Format(Resources.InvalidServerAddress, value));
        return null;
    }
}
