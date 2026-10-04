namespace Mayfly.Client;

internal sealed class MayflyClientOptions
{
    public const string SectionName = "Mayfly";

    public Uri? BaseAddress { get; set; }

    public TimeSpan Timeout { get; set; } = System.Threading.Timeout.InfiniteTimeSpan;
}
