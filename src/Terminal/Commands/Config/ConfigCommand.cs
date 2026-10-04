using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace Mayfly.Terminal.Commands.Config;

internal sealed class ConfigCommand : Command
{
    public ConfigCommand(IServiceProvider services) : base("config", "Show or change settings.")
    {
        var urlArgument = new Argument<Uri>("url") { Description = "Server address.", CustomParser = ServerAddress.Parse };

        var setServer = new Command("server", "Save the server address.") { urlArgument };
        setServer.SetAction(parseResult =>
            services.GetRequiredService<SetServerAction>().Execute(parseResult.GetRequiredValue(urlArgument)));

        var getServer = new Command("server", "Print the saved server address.");
        getServer.SetAction(_ => services.GetRequiredService<GetServerAction>().Execute());

        Subcommands.Add(new Command("set", "Save a setting.") { setServer });
        Subcommands.Add(new Command("get", "Print a setting.") { getServer });
    }
}
