namespace Mayfly.Client;

internal sealed class MayflyClientOptions
{
    public const string SectionName = "Mayfly";

    public Uri? BaseAddress { get; init; }

    public TimeSpan Timeout { get; init; } = System.Threading.Timeout.InfiniteTimeSpan;
}
