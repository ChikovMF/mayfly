using Mayfly.Client;
using Mayfly.Terminal.Configuration;
using Mayfly.Terminal.Output;
using Microsoft.Extensions.Configuration;

namespace Mayfly.Terminal.Commands.Upload;

internal sealed class UploadAction(IMayflyClient client, IConfiguration configuration, ConsoleOutput output)
{
    public async Task<int> ExecuteAsync(FileInfo file, CancellationToken cancellationToken)
    {
        await using var content = file.OpenRead();
        var uploaded = await client.UploadAsync(content, file.Name, cancellationToken: cancellationToken);

        var server = new Uri(configuration[ConfigFile.ServerKey]!);
        var link = new Uri(server, $"api/files/{Uri.EscapeDataString(uploaded.Id)}").ToString();

        if (output.IsRedirected)
        {
            output.Value(link);
            return 0;
        }

        output.Success($"Uploaded {file.Name} ({ConsoleOutput.FormatSize(file.Length)})");
        output.NewLine();
        output.Fields(("Link", link), ("ID", uploaded.Id));
        return 0;
    }
}
