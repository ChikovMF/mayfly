using System.CommandLine;
using System.Globalization;
using Mayfly.Terminal.Configuration;
using Mayfly.Terminal.Properties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mayfly.Terminal.Commands.Upload;

internal sealed class UploadCommand : Command
{
    public UploadCommand(ServerOption serverOption, IConfiguration configuration, IServiceProvider services)
        : base("upload", Resources.UploadDescription)
    {
        var pathArgument = new Argument<FileSystemInfo>("path") { Description = Resources.UploadFileArgumentDescription };
        pathArgument.Validators.Add(result =>
        {
            if (result.GetValueOrDefault<FileSystemInfo>() is { Exists: false } path)
            {
                result.AddError(string.Format(CultureInfo.CurrentCulture, Resources.UploadFileDoesNotExist, path));
            }
        });

        var expiresInOption = new Option<TimeSpan?>("--expires-in", "-e")
        {
            Description = Resources.UploadExpiresInOptionDescription,
            HelpName = "period",
            CustomParser = ExpirationPeriod.Parse,
        };

        var passwordOption = new Option<bool>("--password", "-p") { Description = Resources.UploadPasswordOptionDescription };

        Arguments.Add(pathArgument);
        Options.Add(expiresInOption);
        Options.Add(passwordOption);
        Options.Add(serverOption);
        Validators.Add(serverOption.ValidateAvailable);

        SetAction((parseResult, cancellationToken) =>
        {
            if (parseResult.GetValue(serverOption) is { } server)
            {
                configuration[ConfigFile.ServerKey] = server.ToString();
            }

            return services.GetRequiredService<UploadAction>().ExecuteAsync(
                parseResult.GetRequiredValue(pathArgument),
                parseResult.GetValue(expiresInOption),
                parseResult.GetValue(passwordOption),
                cancellationToken);
        });
    }
}
