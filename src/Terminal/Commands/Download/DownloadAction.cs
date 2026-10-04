using Mayfly.Client;
using Mayfly.Terminal.Output;

namespace Mayfly.Terminal.Commands.Download;

internal sealed class DownloadAction(IMayflyClient client, ConsoleOutput output)
{
    public async Task<int> ExecuteAsync(string id, FileInfo? target, bool overwrite, CancellationToken cancellationToken)
    {
        using var file = await client.DownloadAsync(id, cancellationToken);
        if (file is null)
        {
            output.Error($"File {id} not found.");
            return 1;
        }

        var path = target?.ToString() ?? Path.GetFileName(file.Info.FileName);
        if (!overwrite && File.Exists(path))
        {
            output.Error($"{path} already exists. Use --force to overwrite it or --output to save it elsewhere.");
            return 1;
        }

        await using var stream = new FileStream(path, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write);
        try
        {
            await file.Content.CopyToAsync(stream, cancellationToken);
        }
        catch
        {
            await stream.DisposeAsync();
            File.Delete(path);
            throw;
        }

        if (output.IsRedirected)
        {
            output.Value(path);
            return 0;
        }

        output.Success($"Saved {path} ({ConsoleOutput.FormatSize(file.Info.Size)})");
        return 0;
    }
}
