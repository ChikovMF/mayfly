using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.Globalization;
using Mayfly.Client.Exceptions;
using Mayfly.Terminal.Commands.Completion;
using Mayfly.Terminal.Commands.Config;
using Mayfly.Terminal.Commands.Download;
using Mayfly.Terminal.Commands.Info;
using Mayfly.Terminal.Commands.Upload;
using Mayfly.Terminal.Output;
using Mayfly.Terminal.Properties;

namespace Mayfly.Terminal.Commands;

internal sealed class MayflyRootCommand : RootCommand
{
    private readonly ConsoleOutput _output;

    public MayflyRootCommand(
        UploadCommand upload,
        InfoCommand info,
        DownloadCommand download,
        ConfigCommand config,
        CompletionCommand completion,
        ConsoleOutput output)
        : base(Resources.RootDescription)
    {
        _output = output;

        Options.OfType<HelpOption>().Single().Description = Resources.HelpOptionDescription;
        Options.OfType<VersionOption>().Single().Description = Resources.VersionOptionDescription;

        Subcommands.Add(upload);
        Subcommands.Add(info);
        Subcommands.Add(download);
        Subcommands.Add(config);
        Subcommands.Add(completion);
    }

    public async Task<int> RunAsync(string[] args)
    {
        var parseResult = Parse(args);
        if (parseResult.Action is ParseErrorAction parseErrorAction)
        {
            parseErrorAction.ShowHelp = false;
        }

        try
        {
            return await parseResult.InvokeAsync(new InvocationConfiguration { EnableDefaultExceptionHandler = false });
        }
        catch (MayflyRateLimitException ex)
        {
            _output.Error(ex.RetryAfter.HasValue
                ? string.Format(CultureInfo.CurrentCulture, Resources.TooManyRequestsRetryAfter, Math.Ceiling(ex.RetryAfter.Value.TotalSeconds))
                : Resources.TooManyRequests);
        }
        catch (MayflyApiException ex)
        {
            _output.Error(ex.Message, ex.Errors);
        }
        catch (Exception ex) when (ex is AccessDeniedException or ChecksumMismatchException)
        {
            _output.Error(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            _output.Error(string.Format(CultureInfo.CurrentCulture, Resources.CannotReachServer, ex.Message));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _output.Error(ex.Message);
        }
        catch (OperationCanceledException)
        {
            return 130;
        }

        return 1;
    }
}
