using Dapper;
using Npgsql;
using Server.Domain;
using Server.Domain.Abstractions;
using Server.Domain.ValueObjects;

namespace Server.Infrastructure.Persistence;

internal sealed class PostgresSharedFileRepository(NpgsqlDataSource dataSource) : ISharedFileRepository
{
    public async Task AddAsync(SharedFile file, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO shared_files (id, file_name, content_type, size, expires_at)
            VALUES (@Id, @FileName, @ContentType, @Size, @ExpiresAt)
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
            SELECT id AS Id, file_name AS FileName, content_type AS ContentType, size AS Size, expires_at AS ExpiresAt
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

    public async Task<IReadOnlyList<ExpiredFileClaim>> ClaimExpiredAsync(
        int limit,
        TimeSpan lease,
        int maxAttempts,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE shared_files
            SET cleanup_after = now() + @Lease
            WHERE id IN (
                SELECT id
                FROM shared_files
                WHERE expires_at <= now()
                  AND (cleanup_after IS NULL OR cleanup_after <= now())
                  AND cleanup_attempts < @MaxAttempts
                ORDER BY expires_at
                LIMIT @Limit
                FOR UPDATE SKIP LOCKED)
            RETURNING id AS Id, cleanup_attempts AS FailedAttempts
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var claims = await connection.QueryAsync<ExpiredFileClaimRecord>(new CommandDefinition(
            sql,
            new { Limit = limit, Lease = lease, MaxAttempts = maxAttempts },
            cancellationToken: cancellationToken));

        return [.. claims.Select(claim => new ExpiredFileClaim(ShareId.Parse(claim.Id), claim.FailedAttempts))];
    }

    public async Task PostponeDeletionAsync(ShareId id, TimeSpan delay, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE shared_files
            SET cleanup_attempts = cleanup_attempts + 1,
                cleanup_after = now() + @Delay
            WHERE id = @Id
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { Id = id.ToString(), Delay = delay },
            cancellationToken: cancellationToken));
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

    private sealed record ExpiredFileClaimRecord(string Id, int FailedAttempts);
}
