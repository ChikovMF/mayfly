using System.Net.Http.Headers;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Server.Infrastructure.Storage.YandexDisk;

internal sealed class YandexDiskHealthCheck(HttpClient client, IOptions<YandexDiskOptions> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("?fields=total_space", UriKind.Relative));
        request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", options.Value.OAuthToken);
        using var response = await client.SendAsync(request, cancellationToken);

        return response.IsSuccessStatusCode
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy($"Yandex Disk responded with {(int)response.StatusCode} {response.StatusCode}.");
    }
}
