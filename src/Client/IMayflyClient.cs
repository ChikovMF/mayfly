namespace Mayfly.Client;

public interface IMayflyClient
{
    /// <param name="content">File content.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="contentType">MIME type of the file. The server assumes <c>application/octet-stream</c> when omitted.</param>
    /// <exception cref="MayflyApiException">The server rejected the request.</exception>
    Task<UploadedFile> UploadAsync(
        Stream content,
        string fileName,
        string? contentType = null,
        CancellationToken cancellationToken = default);
}
