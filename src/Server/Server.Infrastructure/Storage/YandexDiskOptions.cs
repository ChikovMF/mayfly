namespace Server.Infrastructure.Storage;

internal sealed class YandexDiskOptions
{
    public const string SectionName = $"{StorageOptions.SectionName}:{nameof(StorageProvider.YandexDisk)}";

    public string OAuthToken { get; init; } = "";

    public string? Path { get; init; }

    public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(1);
}
