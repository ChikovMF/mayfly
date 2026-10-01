using Dapper;
using DbUp;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Server.Infrastructure.Persistence;

public static class DatabaseMigrator
{
    private const long LockKey = 7_362_591_048_217_593_001;

    public static void Migrate(string connectionString, ILoggerFactory loggerFactory)
    {
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
