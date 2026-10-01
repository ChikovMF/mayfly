using Dapper;
using DbUp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Server.Domain.Abstractions;

namespace Server.Infrastructure.Persistence;

internal sealed class DatabaseMigrator(IOptions<PostgresOptions> options, ILoggerFactory loggerFactory)
    : IDatabaseMigrator
{
    private const long LockKey = 7_362_591_048_217_593_001;

    public void Migrate()
    {
        var connectionString = options.Value.ConnectionString;

        using var lockConnection = new NpgsqlConnection(connectionString);
        lockConnection.Open();
        lockConnection.Execute("SELECT pg_advisory_lock(@LockKey)", new { LockKey });

        try
        {
            var result = DeployChanges.To
                .PostgresqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(typeof(DatabaseMigrator).Assembly)
                .WithTransactionPerScript()
                .LogTo(loggerFactory)
                .Build()
                .PerformUpgrade();

            if (!result.Successful)
            {
                throw new InvalidOperationException("Database migration failed.", result.Error);
            }
        }
        finally
        {
            lockConnection.Execute("SELECT pg_advisory_unlock(@LockKey)", new { LockKey });
        }
    }
}
