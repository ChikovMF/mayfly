using System.Globalization;
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
            output.Error(string.Format(CultureInfo.CurrentCulture, Resources.FileNotFound, id));
            return 1;
        }

        var path = target?.ToString() ?? Path.GetFileName(file.Info.FileName);
        if (!overwrite && File.Exists(path))
        {
            output.Error(string.Format(CultureInfo.CurrentCulture, Resources.DownloadFileExists, path));
            return 1;
        }

        var stream = new FileStream(path, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write);
        try
        {
            await using (stream)
            {
                await file.Content.CopyToAsync(stream, cancellationToken);
            }
        }
        catch
        {
            File.Delete(path);
            throw;
        }

        if (ConsoleOutput.IsRedirected)
        {
            ConsoleOutput.Value(path);
            return 0;
        }

        output.Success(string.Format(CultureInfo.CurrentCulture, Resources.DownloadCompleted, path, ConsoleOutput.FormatSize(file.Info.Size)));
        return 0;
    }
}
