using System.CommandLine;
using Mayfly.Terminal.Configuration;
using Microsoft.Extensions.Configuration;

namespace Mayfly.Terminal.Commands;

internal sealed class ServerOption : Option<Uri>
{
    public ServerOption(IConfiguration configuration) : base("--server", "-s")
    {
        Description = "Server address for this command only. Defaults to the configured server.";
        HelpName = "url";
        CustomParser = ServerAddress.Parse;
        DefaultValueFactory = result =>
        {
            var configured = configuration[ConfigFile.ServerKey];
            if (string.IsNullOrEmpty(configured))
            {
                result.AddError("Server address is not set. Use --server <url> or 'mayfly config set server <url>'.");
                return null!;
            }

            return ServerAddress.Parse(result, configured)!;
        };
    }
}
