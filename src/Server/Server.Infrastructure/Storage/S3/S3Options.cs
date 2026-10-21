using Server.Infrastructure.Configuration;

namespace Server.Infrastructure.Storage.S3;

internal sealed class S3Options : ISectionOptions
{
    public static string SectionName => $"{StorageOptions.SectionName}:{nameof(StorageProvider.S3)}";

    public string ServiceUrl { get; init; } = "";

    public string AccessKey { get; init; } = "";

    public string SecretKey { get; init; } = "";

    public string BucketName { get; init; } = "";

    public string Region { get; init; } = "us-east-1";

    public bool ForcePathStyle { get; init; } = true;
}
