using System.CommandLine;
using System.CommandLine.Invocation;
using Mayfly.Client;
using Mayfly.Terminal.Commands.Config;
using Mayfly.Terminal.Commands.Download;
using Mayfly.Terminal.Commands.Info;
using Mayfly.Terminal.Commands.Upload;
using Mayfly.Terminal.Output;

namespace Mayfly.Terminal.Commands;

internal sealed class MayflyRootCommand : RootCommand
{
    private readonly ConsoleOutput _output;

    public MayflyRootCommand(
        UploadCommand upload,
        InfoCommand info,
        DownloadCommand download,
        ConfigCommand config,
        ConsoleOutput output)
        : base("Upload and download files on a Mayfly server.")
    {
        _output = output;

        Subcommands.Add(upload);
        Subcommands.Add(info);
        Subcommands.Add(download);
        Subcommands.Add(config);
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
        catch (MayflyApiException ex)
        {
            _output.Error(ex.Message, ex.Errors);
        }
        catch (HttpRequestException ex)
        {
            _output.Error($"Cannot reach the server: {ex.Message}");
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
