using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Minio;
using Server.Domain.Abstractions;
using Server.Infrastructure.Storage.Minio;
using Server.Infrastructure.Storage.YandexDisk;

namespace Server.Infrastructure.Storage;

internal static class StorageDependencyInjection
{
    public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var storage = configuration.GetSection(StorageOptions.SectionName).Get<StorageOptions>();
        switch (storage?.Provider)
        {
            case StorageProvider.Minio:
                services.AddMinioStorage(configuration);
                break;
            case StorageProvider.YandexDisk:
                services.AddYandexDiskStorage(configuration);
                break;
            default:
                throw new InvalidOperationException(
                    $"{StorageOptions.SectionName}:{nameof(StorageOptions.Provider)} must be one of: {string.Join(", ", Enum.GetNames<StorageProvider>())}.");
        }

        return services;
    }

    private static void AddMinioStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MinioOptions>()
            .Bind(configuration.GetSection(MinioOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<MinioOptions>, MinioOptionsValidator>();

        services.AddSingleton<IMinioClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MinioOptions>>().Value;
            return new MinioClient()
                .WithEndpoint(options.Endpoint)
                .WithCredentials(options.AccessKey, options.SecretKey)
                .WithSSL(options.UseSsl)
                .Build();
        });
        services.AddSingleton<IFileStorage, MinioFileStorage>();
    }

    private static void AddYandexDiskStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<YandexDiskOptions>()
            .Bind(configuration.GetSection(YandexDiskOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<YandexDiskOptions>, YandexDiskOptionsValidator>();

        services.AddHttpClient<IFileStorage, YandexDiskFileStorage>(client =>
        {
            client.BaseAddress = YandexDiskFileStorage.ApiAddress;
            client.Timeout = Timeout.InfiniteTimeSpan;
        });
    }
}
