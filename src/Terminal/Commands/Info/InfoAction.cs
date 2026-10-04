using Mayfly.Client;
using Mayfly.Terminal.Output;

namespace Mayfly.Terminal.Commands.Info;

internal sealed class InfoAction(IMayflyClient client, ConsoleOutput output)
{
    public async Task<int> ExecuteAsync(string id, CancellationToken cancellationToken)
    {
        var info = await client.GetInfoAsync(id, cancellationToken);
        if (info is null)
        {
            output.Error($"File {id} not found.");
            return 1;
        }

        var size = ConsoleOutput.FormatSize(info.Size);
        if (info.Size >= 1024)
        {
            size += $" ({info.Size} bytes)";
        }

        output.Fields(("Name", info.FileName), ("Type", info.ContentType), ("Size", size));
        return 0;
    }
}
