namespace Server.Infrastructure.Storage.YandexDisk;

internal sealed class YandexDiskOptions
{
    public const string SectionName = $"{StorageOptions.SectionName}:{nameof(StorageProvider.YandexDisk)}";

    public string OAuthToken { get; init; } = "";
}
