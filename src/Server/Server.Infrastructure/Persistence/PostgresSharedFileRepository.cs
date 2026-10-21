using Dapper;
using Npgsql;
using Server.Domain;
using Server.Domain.Abstractions;
using Server.Domain.ValueObjects;

namespace Server.Infrastructure.Persistence;

internal sealed class PostgresSharedFileRepository(NpgsqlDataSource dataSource) : ISharedFileRepository
{
    private static readonly TimeSpan UploadDeadline = TimeSpan.FromHours(24);

    public async Task<bool> AddAsync(SharedFile file, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO shared_files (id, file_name, content_type, size, expires_at, password_hash, downloads_left, sha256, cleanup_after)
            VALUES (@Id, @FileName, @ContentType, @Size, @ExpiresAt, @PasswordHash, @DownloadsLeft, @Sha256, now() + @UploadDeadline)
            ON CONFLICT (id) DO NOTHING
            """;

        var parameters = new DynamicParameters(SharedFileRecord.From(file));
        parameters.Add("UploadDeadline", UploadDeadline);

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var inserted = await connection.ExecuteAsync(new CommandDefinition(
            sql,
            parameters,
            cancellationToken: cancellationToken));

        return inserted == 1;
    }

    public async Task<bool> CompleteAsync(ShareId id, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE shared_files
            SET completed_at = now(), cleanup_after = NULL
            WHERE id = @Id AND completed_at IS NULL
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var updated = await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { Id = id.ToString() },
            cancellationToken: cancellationToken));

        return updated == 1;
    }

    public async Task<SharedFile?> GetAsync(ShareId id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS Id, file_name AS FileName, content_type AS ContentType, size AS Size, expires_at AS ExpiresAt,
                   password_hash AS PasswordHash, downloads_left AS DownloadsLeft, sha256 AS Sha256
            FROM shared_files
            WHERE id = @Id AND completed_at IS NOT NULL
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var record = await connection.QuerySingleOrDefaultAsync<SharedFileRecord>(new CommandDefinition(
            sql,
            new { Id = id.ToString() },
            cancellationToken: cancellationToken));

        return record?.ToDomain();
    }

    public async Task<DownloadsLeft?> SpendDownloadAsync(ShareId id, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE shared_files
            SET downloads_left = downloads_left - 1,
                downloads_exhausted_at = CASE WHEN downloads_left = 1 THEN now() END
            WHERE id = @Id AND downloads_left > 0
            RETURNING downloads_left
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var count = await connection.QuerySingleOrDefaultAsync<int?>(new CommandDefinition(
            sql,
            new { Id = id.ToString() },
            cancellationToken: cancellationToken));

        return count.HasValue ? DownloadsLeft.Parse(count.Value) : null;
    }

    public async Task<IReadOnlyList<ExpiredFileClaim>> ClaimExpiredAsync(
        int limit,
        TimeSpan lease,
        TimeSpan exhaustedDelay,
        int maxAttempts,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE shared_files
            SET cleanup_after = now() + @Lease
            WHERE id IN (
                SELECT id
                FROM shared_files
                WHERE (expires_at <= now() OR downloads_exhausted_at <= now() - @ExhaustedDelay OR completed_at IS NULL)
                  AND (cleanup_after IS NULL OR cleanup_after <= now())
                  AND cleanup_attempts < @MaxAttempts
                ORDER BY LEAST(expires_at, downloads_exhausted_at)
                LIMIT @Limit
                FOR UPDATE SKIP LOCKED)
            RETURNING id AS Id, cleanup_attempts AS FailedAttempts, COALESCE(expires_at <= now(), false) AS Expired,
                           completed_at IS NULL AS Unfinished
            """;

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        var claims = await connection.QueryAsync<ExpiredFileClaimRecord>(new CommandDefinition(
            sql,
            new { Limit = limit, Lease = lease, ExhaustedDelay = exhaustedDelay, MaxAttempts = maxAttempts },
            cancellationToken: cancellationToken));

        return [.. claims.Select(claim => new ExpiredFileClaim(
            ShareId.Parse(claim.Id),
            claim.FailedAttempts,
            ReasonOf(claim)))];
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

    private static FileDeletionReason ReasonOf(ExpiredFileClaimRecord claim)
    {
        if (claim.Unfinished)
        {
            return FileDeletionReason.UploadAbandoned;
        }

        return claim.Expired ? FileDeletionReason.Expired : FileDeletionReason.DownloadsExhausted;
    }

    private sealed record ExpiredFileClaimRecord(string Id, int FailedAttempts, bool Expired, bool Unfinished);
}
