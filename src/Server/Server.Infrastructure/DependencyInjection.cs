using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Minio;
using Server.Domain.Abstractions;
using Server.Infrastructure.Persistence;
using Server.Infrastructure.Security;
using Server.Infrastructure.Storage;

namespace Server.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PostgresOptions>().Bind(configuration.GetSection(PostgresOptions.SectionName));
        services.AddOptions<MinioOptions>().Bind(configuration.GetSection(MinioOptions.SectionName));

        services.AddNpgsqlDataSource(
            configuration.GetSection(PostgresOptions.SectionName).Get<PostgresOptions>()?.ConnectionString ?? "");
        services.AddSingleton<IDatabaseMigrator, DatabaseMigrator>();
        services.AddSingleton<ISharedFileRepository, PostgresSharedFileRepository>();

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

        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        return services;
    }
}
