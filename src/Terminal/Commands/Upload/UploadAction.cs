using Mayfly.Client;
using Mayfly.Terminal.Output;
using Mayfly.Terminal.Properties;

namespace Mayfly.Terminal.Commands.Upload;

internal sealed class UploadAction(IMayflyClient client, ConsoleOutput output)
{
    public async Task<int> ExecuteAsync(FileInfo file, CancellationToken cancellationToken)
    {
        await using var content = file.OpenRead();
        var uploaded = await client.UploadAsync(content, file.Name, cancellationToken: cancellationToken);

        if (output.IsRedirected)
        {
            output.Value(uploaded.Id);
            return 0;
        }

        output.Success(string.Format(Resources.UploadCompleted, file.Name, ConsoleOutput.FormatSize(file.Length)));
        output.NewLine();
        output.Fields((Resources.UploadIdLabel, uploaded.Id));
        return 0;
    }
}
