using Amazon.Runtime;
using Amazon.S3;
using Mayfly.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Server.Infrastructure.Persistence;
using Server.Infrastructure.Storage;
using Server.Infrastructure.Storage.S3;
using Testcontainers.Minio;
using Testcontainers.PostgreSql;

namespace Integration.Tests;

public sealed class MayflyFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string BucketName = "mayfly";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();
    private readonly MinioContainer _minio = new MinioBuilder("pgsty/minio:latest").Build();
    private ServiceProvider? _clientServices;

    public IMayflyClient Client => _clientServices!.GetRequiredService<IMayflyClient>();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _minio.StartAsync());
        await CreateBucketAsync();

        UseKestrel(0);
        StartServer();

        var clientConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Mayfly:BaseAddress"] = ClientOptions.BaseAddress.ToString() })
            .Build();
        _clientServices = new ServiceCollection().AddMayflyClient(clientConfiguration).BuildServiceProvider();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting($"{PostgresOptions.SectionName}:{nameof(PostgresOptions.ConnectionString)}", _postgres.GetConnectionString());
        builder.UseSetting($"{StorageOptions.SectionName}:{nameof(StorageOptions.Provider)}", nameof(StorageProvider.S3));
        builder.UseSetting($"{S3Options.SectionName}:{nameof(S3Options.ServiceUrl)}", _minio.GetConnectionString());
        builder.UseSetting($"{S3Options.SectionName}:{nameof(S3Options.AccessKey)}", _minio.GetAccessKey());
        builder.UseSetting($"{S3Options.SectionName}:{nameof(S3Options.SecretKey)}", _minio.GetSecretKey());
        builder.UseSetting($"{S3Options.SectionName}:{nameof(S3Options.BucketName)}", BucketName);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_clientServices is not null)
            await _clientServices.DisposeAsync();
        await DisposeAsync();
        await _postgres.DisposeAsync();
        await _minio.DisposeAsync();
    }

    private async Task CreateBucketAsync()
    {
        var config = new AmazonS3Config { ServiceURL = _minio.GetConnectionString(), ForcePathStyle = true };
        using var s3 = new AmazonS3Client(new BasicAWSCredentials(_minio.GetAccessKey(), _minio.GetSecretKey()), config);

        await s3.PutBucketAsync(BucketName);
    }
}
