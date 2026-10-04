using System.CommandLine;
using Mayfly.Terminal.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mayfly.Terminal.Commands.Upload;

internal sealed class UploadCommand : Command
{
    public UploadCommand(ServerOption serverOption, IConfiguration configuration, IServiceProvider services)
        : base("upload", "Upload a file and print its link.")
    {
        var fileArgument = new Argument<FileInfo>("file") { Description = "File to upload." };
        fileArgument.Validators.Add(result =>
        {
            if (result.GetValueOrDefault<FileInfo>() is { Exists: false } file)
            {
                result.AddError($"File not found: '{file}'.");
            }
        });

        Arguments.Add(fileArgument);
        Options.Add(serverOption);

        SetAction((parseResult, cancellationToken) =>
        {
            configuration[ConfigFile.ServerKey] = parseResult.GetRequiredValue(serverOption).ToString();

            return services.GetRequiredService<UploadAction>()
                .ExecuteAsync(parseResult.GetRequiredValue(fileArgument), cancellationToken);
        });
    }
}
