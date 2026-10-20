using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Server.Host.Files;

namespace Server.Host.RateLimiting;

internal sealed class PasswordRateLimiterPolicy(IOptions<PasswordRateLimitOptions> options) : IRateLimiterPolicy<string>
{
    public const string Name = "password";

    private readonly PasswordRateLimitOptions _options = options.Value;

    public Func<OnRejectedContext, CancellationToken, ValueTask> OnRejected => WriteTooManyRequestsAsync;

    public RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        if (!httpContext.Request.Headers.ContainsKey(FilesEndpoints.PasswordHeader))
        {
            return RateLimitPartition.GetNoLimiter(string.Empty);
        }

        return RateLimitPartition.GetTokenBucketLimiter(
            ClientKey.From(httpContext.Connection.RemoteIpAddress),
            _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = _options.TokenLimit,
                TokensPerPeriod = _options.TokensPerPeriod,
                ReplenishmentPeriod = _options.ReplenishmentPeriod,
                QueueLimit = 0,
            });
    }

    private static ValueTask WriteTooManyRequestsAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var response = context.HttpContext.Response;
        response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        return ValueTask.CompletedTask;
    }
}
