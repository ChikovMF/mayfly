using Server.Domain.ValueObjects;

namespace Server.Domain.Abstractions;

public interface ISharedFileRepository
{
    /// <summary>Adds the file as an unfinished upload, which <see cref="GetAsync"/> does not return until <see cref="CompleteAsync"/>.</summary>
    /// <returns><see langword="false"/> if a file with the same id already exists.</returns>
    Task<bool> AddAsync(SharedFile file, CancellationToken cancellationToken);

    /// <returns><see langword="false"/> if there is no unfinished upload with this id (it was claimed for deletion).</returns>
    Task<bool> CompleteAsync(ShareId id, CancellationToken cancellationToken);

    Task<SharedFile?> GetAsync(ShareId id, CancellationToken cancellationToken);

    /// <returns>The number of downloads left after this one, or <see langword="null"/> if none was left to spend.</returns>
    Task<DownloadsLeft?> SpendDownloadAsync(ShareId id, CancellationToken cancellationToken);

    /// <summary>Claims files that are expired, abandoned unfinished uploads, or files whose downloads ran out at least <paramref name="exhaustedDelay"/> ago.</summary>
    Task<IReadOnlyList<ExpiredFileClaim>> ClaimExpiredAsync(
        int limit,
        TimeSpan lease,
        TimeSpan exhaustedDelay,
        int maxAttempts,
        CancellationToken cancellationToken);

    Task PostponeDeletionAsync(ShareId id, TimeSpan delay, CancellationToken cancellationToken);

    Task DeleteAsync(ShareId id, CancellationToken cancellationToken);
}
