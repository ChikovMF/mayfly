namespace Server.Infrastructure.Storage;

internal sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public StorageProvider Provider { get; init; } = StorageProvider.S3;
}
