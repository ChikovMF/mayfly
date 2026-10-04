using System.CommandLine;
using System.CommandLine.Invocation;
using Mayfly.Client;
using Mayfly.Terminal;
using Mayfly.Terminal.Commands;

var serverOption = new ServerOption();

var rootCommand = new RootCommand("Share files by link.")
{
    UploadCommand.Create(serverOption),
};

var parseResult = rootCommand.Parse(args);
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
    Output.Error(ex.Message, ex.Errors);
    return 1;
}
catch (HttpRequestException ex)
{
    Output.Error($"Cannot reach the server: {ex.Message}");
    return 1;
}
catch (IOException ex)
{
    Output.Error(ex.Message);
    return 1;
}
catch (UnauthorizedAccessException ex)
{
    Output.Error(ex.Message);
    return 1;
}
catch (OperationCanceledException)
{
    return 130;
}
