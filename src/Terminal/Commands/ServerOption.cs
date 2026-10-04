using System.CommandLine;
using System.CommandLine.Parsing;
using Mayfly.Terminal.Configuration;
using Microsoft.Extensions.Configuration;

namespace Mayfly.Terminal.Commands;

internal sealed class ServerOption : Option<Uri>
{
    private readonly IConfiguration _configuration;

    public ServerOption(IConfiguration configuration) : base("--server", "-s")
    {
        _configuration = configuration;

        Description = "Server address for this command only. Defaults to the configured server.";
        HelpName = "url";
        CustomParser = ServerAddress.Parse;
    }

    public void ValidateAvailable(CommandResult result)
    {
        if (result.GetResult(this) is null && string.IsNullOrEmpty(_configuration[ConfigFile.ServerKey]))
        {
            result.AddError("Server address is not set. Use --server <url> or 'mayfly config set server <url>'.");
        }
    }
}
