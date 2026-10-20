namespace Server.Host.Cleanup;

internal sealed class CleanupOptions
{
    public const string SectionName = "Cleanup";

    public TimeSpan Interval { get; init; } = TimeSpan.FromMinutes(1);

    public TimeSpan Lease { get; init; } = TimeSpan.FromMinutes(5);

    public TimeSpan ExhaustedFileDelay { get; init; } = TimeSpan.FromHours(1);

    public int MaxAttempts { get; init; } = 15;
}
