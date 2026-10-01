namespace Server.Infrastructure.Persistence;

internal sealed class PostgresOptions
{
    public const string SectionName = "Postgres";

    public string ConnectionString { get; init; } = "";
}
