using Mayfly.Client;
using Mayfly.Terminal.Output;
using Mayfly.Terminal.Properties;

namespace Mayfly.Terminal.Commands.Download;

internal sealed class DownloadAction(IMayflyClient client, ConsoleOutput output)
{
    public async Task<int> ExecuteAsync(string id, FileInfo? target, bool overwrite, CancellationToken cancellationToken)
    {
        using var file = await client.DownloadAsync(id, cancellationToken);
        if (file is null)
        {
            output.Error(string.Format(Resources.FileNotFound, id));
            return 1;
        }

        var path = target?.ToString() ?? Path.GetFileName(file.Info.FileName);
        if (!overwrite && File.Exists(path))
        {
            output.Error(string.Format(Resources.DownloadFileExists, path));
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

        output.Success(string.Format(Resources.DownloadCompleted, path, ConsoleOutput.FormatSize(file.Info.Size)));
        return 0;
    }
}
