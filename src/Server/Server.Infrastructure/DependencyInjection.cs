using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Server.Domain.Abstractions;
using Server.Infrastructure.Configuration;
using Server.Infrastructure.Persistence;
using Server.Infrastructure.Security;
using Server.Infrastructure.Storage;

namespace Server.Infrastructure;

public static class DependencyInjection
{
    internal static readonly TimeSpan HealthCheckTimeout = TimeSpan.FromSeconds(5);

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PostgresOptions>(configuration);

        services.AddNpgsqlDataSource(
            configuration.GetSection(PostgresOptions.SectionName).Get<PostgresOptions>()?.ConnectionString ?? "");
        services.AddSingleton<IDatabaseMigrator, DatabaseMigrator>();
        services.AddSingleton<ISharedFileRepository, PostgresSharedFileRepository>();
        services.AddHealthChecks().AddCheck<PostgresHealthCheck>("database", timeout: HealthCheckTimeout);

        services.AddFileStorage(configuration);

        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        return services;
    }
}
