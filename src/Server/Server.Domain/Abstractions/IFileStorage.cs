using Server.Domain.ValueObjects;

namespace Server.Domain.Abstractions;

public interface IFileStorage
{
    /// <param name="id">The id the content is stored under.</param>
    /// <param name="content">The content to store; the stream does not need to be seekable.</param>
    /// <param name="size">The number of bytes <paramref name="content"/> holds.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task SaveAsync(ShareId id, Stream content, FileSize size, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(ShareId id, CancellationToken cancellationToken);

    Task CopyToAsync(ShareId id, Stream destination, CancellationToken cancellationToken);

    Task DeleteAsync(ShareId id, CancellationToken cancellationToken);
}
