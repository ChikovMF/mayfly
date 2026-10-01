using Dapper;
using Npgsql;
using Server.Domain;
using Server.Domain.Abstractions;
using Server.Domain.ValueObjects;

namespace Server.Infrastructure;

internal sealed class PostgresSharedFileRepository(NpgsqlDataSource dataSource) : ISharedFileRepository
{
    public async Task AddAsync(SharedFile file, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO shared_files (id, file_name, content_type, size)
            VALUES (@Id, @FileName, @ContentType, @Size)
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            SharedFileRecord.From(file),
            cancellationToken: cancellationToken));
    }

    public async Task<SharedFile?> GetAsync(ShareId id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS Id, file_name AS FileName, content_type AS ContentType, size AS Size
            FROM shared_files
            WHERE id = @Id
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var record = await connection.QuerySingleOrDefaultAsync<SharedFileRecord>(new CommandDefinition(
            sql,
            new { Id = id.ToString() },
            cancellationToken: cancellationToken));

        return record?.ToDomain();
    }

    public async Task DeleteAsync(ShareId id, CancellationToken cancellationToken)
    {
        const string sql = "DELETE FROM shared_files WHERE id = @Id";

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { Id = id.ToString() },
            cancellationToken: cancellationToken));
    }
}
