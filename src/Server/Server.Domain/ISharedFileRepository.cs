namespace Server.Domain;

public interface ISharedFileRepository
{
    Task AddAsync(SharedFile file, CancellationToken cancellationToken);

    Task<SharedFile?> GetAsync(ShareId id, CancellationToken cancellationToken);

    Task DeleteAsync(ShareId id, CancellationToken cancellationToken);
}
