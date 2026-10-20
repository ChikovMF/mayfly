using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Server.Host.Endpoints;

internal static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false, ResponseWriter = WriteStatusAsync });
        app.MapHealthChecks("/health", new HealthCheckOptions { ResponseWriter = WriteReportAsync });

        return app;
    }

    private static Task WriteStatusAsync(HttpContext context, HealthReport report) =>
        context.Response.WriteAsJsonAsync(new HealthResponse(report.Status.ToString(), null));

    private static Task WriteReportAsync(HttpContext context, HealthReport report) =>
        context.Response.WriteAsJsonAsync(new HealthResponse(
            report.Status.ToString(),
            report.Entries.ToDictionary(entry => entry.Key, entry => entry.Value.Status.ToString())));
}
