namespace Server.Host;

internal sealed class CleanupOptions
{
    public const string SectionName = "Cleanup";

    public TimeSpan Interval { get; init; } = TimeSpan.FromMinutes(1);
}
