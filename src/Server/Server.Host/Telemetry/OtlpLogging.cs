using System.Reflection;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace Server.Host.Telemetry;

internal static class OtlpLogging
{
    private const string EndpointVariable = "OTEL_EXPORTER_OTLP_ENDPOINT";
    private const string ServiceName = "mayfly";

    public static IServiceCollection AddOtlpLogging(this IServiceCollection services, IConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration[EndpointVariable]))
        {
            return services;
        }

        var version = typeof(OtlpLogging).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .Clear()
                .AddService(ServiceName, serviceVersion: version)
                .AddTelemetrySdk()
                .AddEnvironmentVariableDetector())
            .WithLogging(logging => logging.AddOtlpExporter());

        return services;
    }
}
