namespace Server.Domain;

public interface IFileStorage
{
    Task SaveAsync(SharedFile file, Stream content, CancellationToken cancellationToken);

    Task<FileContent?> OpenReadAsync(ShareId id, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(ShareId id, CancellationToken cancellationToken);
}
