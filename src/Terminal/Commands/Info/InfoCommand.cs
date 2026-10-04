using System.CommandLine;
using Mayfly.Terminal.Configuration;
using Mayfly.Terminal.Properties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mayfly.Terminal.Commands.Info;

internal sealed class InfoCommand : Command
{
    public InfoCommand(ServerOption serverOption, IConfiguration configuration, IServiceProvider services)
        : base("info", Resources.InfoDescription)
    {
        var idArgument = new Argument<string>("id") { Description = Resources.FileIdArgumentDescription };

        Arguments.Add(idArgument);
        Options.Add(serverOption);
        Validators.Add(serverOption.ValidateAvailable);

        SetAction((parseResult, cancellationToken) =>
        {
            if (parseResult.GetValue(serverOption) is { } server)
            {
                configuration[ConfigFile.ServerKey] = server.ToString();
            }

            return services.GetRequiredService<InfoAction>()
                .ExecuteAsync(parseResult.GetRequiredValue(idArgument), cancellationToken);
        });
    }
}
