using Server.Infrastructure.Configuration;

namespace Server.Infrastructure.Persistence;

internal sealed class PostgresOptions : ISectionOptions
{
    public static string SectionName => "Postgres";

    public string ConnectionString { get; init; } = "";
}
