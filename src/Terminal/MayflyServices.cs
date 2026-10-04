using Mayfly.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mayfly.Terminal;

internal static class MayflyServices
{
    public static ServiceProvider Create(Uri server)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Mayfly:BaseAddress"] = server.ToString() })
            .Build();

        return new ServiceCollection().AddMayflyClient(configuration).BuildServiceProvider();
    }
}
