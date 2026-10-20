using Server.Infrastructure.Configuration;

namespace Server.Infrastructure.Storage.YandexDisk;

internal sealed class YandexDiskOptions : ISectionOptions
{
    public static string SectionName => $"{StorageOptions.SectionName}:{nameof(StorageProvider.YandexDisk)}";

    public string OAuthToken { get; init; } = "";
}
