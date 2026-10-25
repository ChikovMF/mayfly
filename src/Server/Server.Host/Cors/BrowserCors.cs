using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Server.Host.Files;
using Server.Infrastructure.Configuration;

namespace Server.Host.Cors;

internal static class BrowserCors
{
    private static readonly TimeSpan PreflightMaxAge = TimeSpan.FromMinutes(10);

    public static IServiceCollection AddBrowserCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<BrowserCorsOptions, BrowserCorsOptionsValidator>(configuration);
        services.AddCors();
        services.AddOptions<CorsOptions>().Configure<IOptions<BrowserCorsOptions>>((cors, browser) =>
            cors.AddDefaultPolicy(policy => policy
                .WithOrigins([.. browser.Value.AllowedOrigins.Select(ToOrigin)])
                .WithMethods(HttpMethods.Get, HttpMethods.Head, HttpMethods.Post)
                .WithHeaders(
                    HeaderNames.ContentDisposition,
                    HeaderNames.ContentType,
                    FilesEndpoints.ExpiresAtHeader,
                    FilesEndpoints.MaxDownloadsHeader,
                    FilesEndpoints.PasswordHeader,
                    FilesEndpoints.ReprDigestHeader)
                .WithExposedHeaders(
                    HeaderNames.ContentDisposition,
                    FilesEndpoints.ReprDigestHeader,
                    FilesEndpoints.ExpiresAtHeader,
                    FilesEndpoints.DownloadsLeftHeader,
                    HeaderNames.RetryAfter)
                .SetPreflightMaxAge(PreflightMaxAge)));

        return services;
    }

    private static string ToOrigin(string url) => new Uri(url).GetLeftPart(UriPartial.Authority);
}
