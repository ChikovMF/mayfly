namespace Server.Host.Files;

internal sealed class UploadOptions
{
    public const string SectionName = "Upload";

    public long MaxRequestBodySize { get; init; } = 4L * 1024 * 1024 * 1024;
}
