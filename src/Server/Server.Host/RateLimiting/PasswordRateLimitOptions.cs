using Server.Infrastructure.Configuration;

namespace Server.Host.RateLimiting;

internal sealed class PasswordRateLimitOptions : ISectionOptions
{
    public static string SectionName => "PasswordRateLimit";

    public int TokenLimit { get; init; } = 10;

    public int TokensPerPeriod { get; init; } = 1;

    public TimeSpan ReplenishmentPeriod { get; init; } = TimeSpan.FromSeconds(6);
}
