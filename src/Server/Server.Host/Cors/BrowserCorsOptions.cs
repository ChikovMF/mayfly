using Server.Infrastructure.Configuration;

namespace Server.Host.Cors;

internal sealed class BrowserCorsOptions : ISectionOptions
{
    public static string SectionName => "Cors";

    public string[] AllowedOrigins { get; init; } = [];
}
