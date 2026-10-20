using Server.Infrastructure.Configuration;

namespace Server.Infrastructure.Storage.Minio;

internal sealed class MinioOptions : ISectionOptions
{
    public static string SectionName => $"{StorageOptions.SectionName}:{nameof(StorageProvider.Minio)}";

    public string Endpoint { get; init; } = "";

    public string AccessKey { get; init; } = "";

    public string SecretKey { get; init; } = "";

    public bool UseSsl { get; init; }

    public string BucketName { get; init; } = "";
}
