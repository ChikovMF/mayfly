using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Server.Domain.Abstractions;
using Server.Infrastructure.Configuration;
using Server.Infrastructure.Storage.S3;
using Server.Infrastructure.Storage.YandexDisk;

namespace Server.Infrastructure.Storage;

internal static class StorageDependencyInjection
{
    private const string HealthCheckName = "storage";

    public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var storage = configuration.GetSection(StorageOptions.SectionName).Get<StorageOptions>() ?? new StorageOptions();
        switch (storage.Provider)
        {
            case StorageProvider.S3:
                services.AddS3Storage(configuration);
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

    private static void AddS3Storage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<S3Options, S3OptionsValidator>(configuration);

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<S3Options>>().Value;
            var config = new AmazonS3Config
            {
                ServiceURL = options.ServiceUrl,
                AuthenticationRegion = options.Region,
                ForcePathStyle = options.ForcePathStyle,
                RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
                ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED,
            };
            return new AmazonS3Client(new BasicAWSCredentials(options.AccessKey, options.SecretKey), config);
        });
        services.AddSingleton<IFileStorage, S3FileStorage>();
        services.AddHealthChecks().AddCheck<S3HealthCheck>(HealthCheckName, timeout: DependencyInjection.HealthCheckTimeout);
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
