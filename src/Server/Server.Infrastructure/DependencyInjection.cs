using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        services.AddNpgsqlDataSource(
            configuration.GetSection(PostgresOptions.SectionName).Get<PostgresOptions>()?.ConnectionString ?? "");
        services.AddSingleton<IDatabaseMigrator, DatabaseMigrator>();
        services.AddSingleton<ISharedFileRepository, PostgresSharedFileRepository>();

        services.AddFileStorage(configuration);

        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        return services;
    }
}
