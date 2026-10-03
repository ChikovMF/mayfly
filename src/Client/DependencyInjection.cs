using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Mayfly.Client;

public static class DependencyInjection
{
    public static IServiceCollection AddMayflyClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MayflyClientOptions>().Bind(configuration.GetSection(MayflyClientOptions.SectionName));

        services.AddHttpClient<IMayflyClient, MayflyClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<MayflyClientOptions>>().Value;
            client.BaseAddress = options.BaseAddress
                                 ?? throw new InvalidOperationException(
                                     $"{MayflyClientOptions.SectionName}:{nameof(MayflyClientOptions.BaseAddress)} is not configured.");
            client.Timeout = options.Timeout;
        });

        return services;
    }
}
