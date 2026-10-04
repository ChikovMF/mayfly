using Mayfly.Client;
using Mayfly.Terminal.Commands;
using Mayfly.Terminal.Commands.Config;
using Mayfly.Terminal.Commands.Upload;
using Mayfly.Terminal.Output;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mayfly.Terminal;

internal static class DependencyInjection
{
    public static IServiceCollection AddTerminal(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(configuration);
        services.AddMayflyClient(configuration);
        services.AddSingleton<ConsoleOutput>();

        services.AddSingleton<MayflyRootCommand>();
        services.AddSingleton<ServerOption>();
        services.AddSingleton<UploadCommand>();
        services.AddSingleton<ConfigCommand>();

        services.AddTransient<UploadAction>();
        services.AddTransient<SetServerAction>();
        services.AddTransient<GetServerAction>();

        return services;
    }
}
