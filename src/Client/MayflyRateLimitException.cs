using System.Net;

namespace Mayfly.Client;

/// <summary>The server refused the request because the client made too many requests.</summary>
public sealed class MayflyRateLimitException(
    string message,
    IReadOnlyList<string> errors,
    TimeSpan? retryAfter)
    : MayflyApiException(HttpStatusCode.TooManyRequests, message, errors)
{
    /// <summary>How long to wait before repeating the request, or <see langword="null"/> if the server did not say.</summary>
    public TimeSpan? RetryAfter { get; } = retryAfter;
}
