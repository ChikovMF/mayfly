using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Server.Infrastructure.Storage.S3;

internal sealed class S3HealthCheck(IAmazonS3 client, IOptions<S3Options> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await client.HeadBucketAsync(new HeadBucketRequest { BucketName = options.Value.BucketName }, cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return HealthCheckResult.Unhealthy($"Bucket '{options.Value.BucketName}' does not exist.");
        }
    }
}
