using Server.Domain.ValueObjects;

namespace Server.Domain.Abstractions;

public interface ISharedFileRepository
{
    /// <returns><see langword="false"/> if a file with the same id already exists.</returns>
    Task<bool> AddAsync(SharedFile file, CancellationToken cancellationToken);

    Task<SharedFile?> GetAsync(ShareId id, CancellationToken cancellationToken);

    Task SetSha256Async(ShareId id, Sha256 sha256, CancellationToken cancellationToken);

    /// <returns>The number of downloads left after this one, or <see langword="null"/> if none was left to spend.</returns>
    Task<DownloadsLeft?> SpendDownloadAsync(ShareId id, CancellationToken cancellationToken);

    /// <summary>Claims files that are expired, or whose downloads ran out at least <paramref name="exhaustedDelay"/> ago.</summary>
    Task<IReadOnlyList<ExpiredFileClaim>> ClaimExpiredAsync(
        int limit,
        TimeSpan lease,
        TimeSpan exhaustedDelay,
        int maxAttempts,
        CancellationToken cancellationToken);

    Task PostponeDeletionAsync(ShareId id, TimeSpan delay, CancellationToken cancellationToken);

    Task DeleteAsync(ShareId id, CancellationToken cancellationToken);
}
