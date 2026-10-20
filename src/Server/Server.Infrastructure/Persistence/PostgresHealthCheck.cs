using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace Server.Infrastructure.Persistence;

internal sealed class PostgresHealthCheck(NpgsqlDataSource dataSource) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        await using var command = dataSource.CreateCommand("SELECT 1");
        await command.ExecuteScalarAsync(cancellationToken);
        return HealthCheckResult.Healthy();
    }
}
