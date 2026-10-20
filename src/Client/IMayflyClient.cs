using Mayfly.Client.Models;

namespace Mayfly.Client;

public interface IMayflyClient
{
    /// <param name="content">File content.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="contentType">MIME type of the file.</param>
    /// <param name="expiresAt">The moment after which the file is unavailable, or <see langword="null"/> to keep it forever.</param>
    /// <param name="password">The password that protects the file, or <see langword="null"/> to leave it unprotected.</param>
    /// <param name="maxDownloads">How many times the file can be downloaded, or <see langword="null"/> for no limit.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="MayflyApiException">The server rejected the request (for example, the file is not valid), or its response could not be understood.</exception>
    /// <exception cref="MayflyRateLimitException">The client made too many requests.</exception>
    /// <exception cref="FormatException"><paramref name="contentType"/> is not a valid MIME type.</exception>
    /// <exception cref="HttpRequestException">The server could not be reached.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled or the request timed out.</exception>
    Task<UploadedFile> UploadAsync(
        Stream content,
        string fileName,
        string? contentType = null,
        DateTimeOffset? expiresAt = null,
        string? password = null,
        int? maxDownloads = null,
        CancellationToken cancellationToken = default);

    /// <returns>Information about the file, or <see langword="null"/> if there is no such file.</returns>
    /// <exception cref="MayflyApiException">The server rejected the request (for example, the file is protected and the password is missing or wrong), or its response could not be understood.</exception>
    /// <exception cref="MayflyRateLimitException">The client made too many requests.</exception>
    /// <exception cref="HttpRequestException">The server could not be reached.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled or the request timed out.</exception>
    Task<RemoteFileInfo?> GetInfoAsync(string id, string? password = null, CancellationToken cancellationToken = default);

    /// <returns>The file, or <see langword="null"/> if there is no such file. The caller must dispose it.</returns>
    /// <exception cref="MayflyApiException">The server rejected the request (for example, the file is protected and the password is missing or wrong), or its response could not be understood.</exception>
    /// <exception cref="MayflyRateLimitException">The client made too many requests.</exception>
    /// <exception cref="HttpRequestException">The server could not be reached.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled or the request timed out.</exception>
    Task<DownloadedFile?> DownloadAsync(string id, string? password = null, CancellationToken cancellationToken = default);
}
