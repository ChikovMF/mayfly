namespace Server.Host;

internal sealed class UploadOptions
{
    public const string SectionName = "Upload";

    public long MaxRequestBodySize { get; init; } = 128 * 1024 * 1024;
}
