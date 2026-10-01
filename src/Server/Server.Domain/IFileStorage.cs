namespace Server.Domain;

public interface IFileStorage
{
    Task SaveAsync(SharedFile file, Stream content, CancellationToken cancellationToken);

    Task<FileContent?> OpenReadAsync(ShareId id, CancellationToken cancellationToken);

    Task DeleteAsync(ShareId id, CancellationToken cancellationToken);
}
