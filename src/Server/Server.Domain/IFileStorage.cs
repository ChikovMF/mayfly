namespace Server.Domain;

public interface IFileStorage
{
    Task SaveAsync(ShareId id, Stream content, CancellationToken cancellationToken);

    Task<Stream?> OpenReadAsync(ShareId id, CancellationToken cancellationToken);

    Task DeleteAsync(ShareId id, CancellationToken cancellationToken);
}
