using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Minio;
using Server.Domain.Abstractions;
using Server.Infrastructure.Configuration;
using Server.Infrastructure.Storage.Minio;
using Server.Infrastructure.Storage.YandexDisk;

namespace Server.Infrastructure.Storage;

internal static class StorageDependencyInjection
{
    private const string HealthCheckName = "storage";

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
        services.AddOptions<MinioOptions, MinioOptionsValidator>(configuration);

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
        services.AddHealthChecks().AddCheck<MinioHealthCheck>(HealthCheckName, timeout: DependencyInjection.HealthCheckTimeout);
    }

    private static void AddYandexDiskStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<YandexDiskOptions, YandexDiskOptionsValidator>(configuration);

        services.AddHttpClient<IFileStorage, YandexDiskFileStorage>(client =>
        {
            client.BaseAddress = YandexDiskFileStorage.ApiAddress;
            client.Timeout = Timeout.InfiniteTimeSpan;
        });
        services.AddHttpClient<YandexDiskHealthCheck>(client => client.BaseAddress = YandexDiskFileStorage.ApiAddress);
        services.AddHealthChecks().AddCheck<YandexDiskHealthCheck>(HealthCheckName, timeout: DependencyInjection.HealthCheckTimeout);
    }
}
