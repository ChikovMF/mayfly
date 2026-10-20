using System.Net;

namespace Mayfly.Client;

/// <summary>The server responded with an unsuccessful status code.</summary>
public class MayflyApiException(
    HttpStatusCode statusCode,
    string message,
    IReadOnlyList<string> errors)
    : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;

    /// <summary>Error messages returned by the server. Empty if there are none.</summary>
    public IReadOnlyList<string> Errors { get; } = errors;
}
