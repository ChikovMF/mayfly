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
        var fileArgument = new Argument<FileInfo>("file") { Description = Resources.UploadFileArgumentDescription };
        fileArgument.Validators.Add(result =>
        {
            if (result.GetValueOrDefault<FileInfo>() is { Exists: false } file)
            {
                result.AddError(string.Format(CultureInfo.CurrentCulture, Resources.UploadFileDoesNotExist, file));
            }
        });

        var expiresInOption = new Option<TimeSpan?>("--expires-in", "-e")
        {
            Description = Resources.UploadExpiresInOptionDescription,
            HelpName = "period",
            CustomParser = ExpirationPeriod.Parse,
        };

        var passwordOption = new Option<bool>("--password", "-p") { Description = Resources.UploadPasswordOptionDescription };

        Arguments.Add(fileArgument);
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
                parseResult.GetRequiredValue(fileArgument),
                parseResult.GetValue(expiresInOption),
                parseResult.GetValue(passwordOption),
                cancellationToken);
        });
    }
}
