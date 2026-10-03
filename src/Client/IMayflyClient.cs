using Mayfly.Client.Models;

namespace Mayfly.Client;

public interface IMayflyClient
{
    /// <param name="content">File content.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="contentType">MIME type of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="MayflyApiException">The server rejected the request.</exception>
    Task<UploadedFile> UploadAsync(
        Stream content,
        string fileName,
        string? contentType = null,
        CancellationToken cancellationToken = default);

    /// <returns>Information about the file, or <see langword="null"/> if there is no such file.</returns>
    /// <exception cref="MayflyApiException">The server rejected the request.</exception>
    Task<RemoteFileInfo?> GetInfoAsync(string id, CancellationToken cancellationToken = default);

    /// <returns>The file, or <see langword="null"/> if there is no such file. The caller must dispose it.</returns>
    /// <exception cref="MayflyApiException">The server rejected the request.</exception>
    Task<DownloadedFile?> DownloadAsync(string id, CancellationToken cancellationToken = default);
}
