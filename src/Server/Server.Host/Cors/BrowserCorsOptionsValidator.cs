using Microsoft.Extensions.Options;

namespace Server.Host.Cors;

internal sealed class BrowserCorsOptionsValidator : IValidateOptions<BrowserCorsOptions>
{
    public ValidateOptionsResult Validate(string? name, BrowserCorsOptions options)
    {
        var failures = new List<string>();

        foreach (var origin in options.AllowedOrigins)
        {
            if (!IsHttpUrl(origin))
            {
                failures.Add($"{BrowserCorsOptions.SectionName}:{nameof(BrowserCorsOptions.AllowedOrigins)} contains '{origin}', which is not an http or https address like 'https://example.com'.");
            }
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static bool IsHttpUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
