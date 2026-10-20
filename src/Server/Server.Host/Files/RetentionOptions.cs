using Server.Infrastructure.Configuration;

namespace Server.Host.Files;

internal sealed class RetentionOptions : ISectionOptions
{
    public static string SectionName => "Retention";

    public bool RequireExpiration { get; init; }

    public TimeSpan? MaxLifetime { get; init; }
}
