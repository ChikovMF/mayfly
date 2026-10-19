using Mayfly.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Minio.DataModel.Args;
using Server.Infrastructure.Persistence;
using Server.Infrastructure.Storage;
using Server.Infrastructure.Storage.Minio;
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
        builder.UseSetting($"{StorageOptions.SectionName}:{nameof(StorageOptions.Provider)}", nameof(StorageProvider.Minio));
        builder.UseSetting($"{MinioOptions.SectionName}:{nameof(MinioOptions.Endpoint)}", MinioEndpoint);
        builder.UseSetting($"{MinioOptions.SectionName}:{nameof(MinioOptions.AccessKey)}", _minio.GetAccessKey());
        builder.UseSetting($"{MinioOptions.SectionName}:{nameof(MinioOptions.SecretKey)}", _minio.GetSecretKey());
        builder.UseSetting($"{MinioOptions.SectionName}:{nameof(MinioOptions.UseSsl)}", "false");
        builder.UseSetting($"{MinioOptions.SectionName}:{nameof(MinioOptions.BucketName)}", BucketName);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_clientServices is not null)
            await _clientServices.DisposeAsync();
        await DisposeAsync();
        await _postgres.DisposeAsync();
        await _minio.DisposeAsync();
    }

    private string MinioEndpoint => $"{_minio.Hostname}:{_minio.GetMappedPublicPort(MinioBuilder.MinioPort)}";

    private async Task CreateBucketAsync()
    {
        using var minio = new MinioClient()
            .WithEndpoint(MinioEndpoint)
            .WithCredentials(_minio.GetAccessKey(), _minio.GetSecretKey())
            .Build();

        await minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(BucketName));
    }
}
