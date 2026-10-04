using System.CommandLine;
using Mayfly.Terminal.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mayfly.Terminal.Commands.Download;

internal sealed class DownloadCommand : Command
{
    public DownloadCommand(ServerOption serverOption, IConfiguration configuration, IServiceProvider services)
        : base("download", "Download a file.")
    {
        var idArgument = new Argument<string>("id") { Description = "File ID." };
        var outputOption = new Option<FileInfo>("--output", "-o")
        {
            Description = "Path to save the file to. Defaults to the file's name in the current directory.",
            HelpName = "path",
        };
        var forceOption = new Option<bool>("--force", "-f") { Description = "Overwrite the file if it already exists." };

        Arguments.Add(idArgument);
        Options.Add(outputOption);
        Options.Add(forceOption);
        Options.Add(serverOption);
        Validators.Add(serverOption.ValidateAvailable);

        SetAction((parseResult, cancellationToken) =>
        {
            if (parseResult.GetValue(serverOption) is { } server)
            {
                configuration[ConfigFile.ServerKey] = server.ToString();
            }

            return services.GetRequiredService<DownloadAction>().ExecuteAsync(
                parseResult.GetRequiredValue(idArgument),
                parseResult.GetValue(outputOption),
                parseResult.GetValue(forceOption),
                cancellationToken);
        });
    }
}
