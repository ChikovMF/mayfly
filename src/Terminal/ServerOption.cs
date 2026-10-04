using System.CommandLine;
using System.CommandLine.Parsing;

namespace Mayfly.Terminal;

internal sealed class ServerOption : Option<Uri>
{
    private const string EnvironmentVariable = "MAYFLY_SERVER";

    public ServerOption() : base("--server", "-s")
    {
        Description = $"Server address. Defaults to the {EnvironmentVariable} environment variable.";
        HelpName = "url";
        DefaultValueFactory = result => Parse(result, Environment.GetEnvironmentVariable(EnvironmentVariable))!;
        CustomParser = result => Parse(result, result.Tokens.Single().Value);
    }

    private static Uri? Parse(ArgumentResult result, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            result.AddError($"Server address is not set. Use --server or the {EnvironmentVariable} environment variable.");
            return null;
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            result.AddError($"'{value}' is not a valid server address.");
            return null;
        }

        return uri;
    }
}
