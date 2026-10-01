namespace Server.Infrastructure.Storage;

internal sealed class MinioOptions
{
    public const string SectionName = "Minio";

    public string Endpoint { get; init; } = "";

    public string AccessKey { get; init; } = "";

    public string SecretKey { get; init; } = "";

    public bool UseSsl { get; init; }

    public string BucketName { get; init; } = "";
}
