using System.CommandLine;
using Mayfly.Terminal.Properties;
using Microsoft.Extensions.DependencyInjection;

namespace Mayfly.Terminal.Commands.Config;

internal sealed class ConfigCommand : Command
{
    public ConfigCommand(IServiceProvider services) : base("config", Resources.ConfigDescription)
    {
        var urlArgument = new Argument<Uri>("url") { Description = Resources.ConfigUrlArgumentDescription, CustomParser = ServerAddress.Parse };

        var setServer = new Command("server", Resources.ConfigSetServerDescription) { urlArgument };
        setServer.SetAction(parseResult =>
            services.GetRequiredService<SetServerAction>().Execute(parseResult.GetRequiredValue(urlArgument)));

        var getServer = new Command("server", Resources.ConfigGetServerDescription);
        getServer.SetAction(_ => services.GetRequiredService<GetServerAction>().Execute());

        Subcommands.Add(new Command("set", Resources.ConfigSetDescription) { setServer });
        Subcommands.Add(new Command("get", Resources.ConfigGetDescription) { getServer });
    }
}
