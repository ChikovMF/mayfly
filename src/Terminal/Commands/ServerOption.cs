using System.CommandLine;
using System.CommandLine.Parsing;
using Mayfly.Terminal.Configuration;
using Mayfly.Terminal.Properties;
using Microsoft.Extensions.Configuration;

namespace Mayfly.Terminal.Commands;

internal sealed class ServerOption : Option<Uri>
{
    private readonly IConfiguration _configuration;

    public ServerOption(IConfiguration configuration) : base("--server", "-s")
    {
        _configuration = configuration;

        Description = Resources.ServerOptionDescription;
        HelpName = "url";
        CustomParser = ServerAddress.Parse;
    }

    public void ValidateAvailable(CommandResult result)
    {
        if (result.GetResult(this) is null && string.IsNullOrEmpty(_configuration[ConfigFile.ServerKey]))
        {
            result.AddError(Resources.ServerNotSet);
        }
    }
}
