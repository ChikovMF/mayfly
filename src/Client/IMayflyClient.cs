using Mayfly.Client.Models;

namespace Mayfly.Client;

public interface IMayflyClient
{
    /// <param name="content">File content.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="contentType">MIME type of the file.</param>
    /// <param name="expiresAt">The moment after which the file is unavailable, or <see langword="null"/> to keep it forever.</param>
    /// <param name="password">The password that protects the file, or <see langword="null"/> to leave it unprotected.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="MayflyApiException">The server rejected the request.</exception>
    Task<UploadedFile> UploadAsync(
        Stream content,
        string fileName,
        string? contentType = null,
        DateTimeOffset? expiresAt = null,
        string? password = null,
        CancellationToken cancellationToken = default);

    /// <returns>Information about the file, or <see langword="null"/> if there is no such file.</returns>
    /// <exception cref="MayflyApiException">The server rejected the request.</exception>
    Task<RemoteFileInfo?> GetInfoAsync(string id, string? password = null, CancellationToken cancellationToken = default);

    /// <returns>The file, or <see langword="null"/> if there is no such file. The caller must dispose it.</returns>
    /// <exception cref="MayflyApiException">The server rejected the request.</exception>
    Task<DownloadedFile?> DownloadAsync(string id, string? password = null, CancellationToken cancellationToken = default);
}
