using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Server.Infrastructure.Storage.Minio;

internal sealed class MinioHealthCheck(IMinioClient client, IOptions<MinioOptions> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var args = new BucketExistsArgs().WithBucket(options.Value.BucketName);

        return await client.BucketExistsAsync(args, cancellationToken)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy($"Bucket '{options.Value.BucketName}' does not exist.");
    }
}
