using System.Globalization;
using System.Security.Cryptography;
using Mayfly.Client;
using Mayfly.Terminal.Output;
using Mayfly.Terminal.Properties;

namespace Mayfly.Terminal.Commands.Download;

internal sealed class DownloadAction(IMayflyClient client, ConsoleOutput output)
{
    private const int BufferSize = 81920;

    public async Task<int> ExecuteAsync(string id, FileInfo? target, bool overwrite, CancellationToken cancellationToken)
    {
        using var file = await PasswordRetry.RunAsync(password => client.DownloadAsync(id, password, cancellationToken));
        if (file is null)
        {
            output.Error(string.Format(CultureInfo.CurrentCulture, Resources.FileNotFound, id));
            return 1;
        }

        var path = ResolvePath(target, Path.GetFileName(file.Info.FileName));
        if (!overwrite && File.Exists(path))
        {
            output.Error(string.Format(CultureInfo.CurrentCulture, Resources.DownloadFileExists, path));
            return 1;
        }

        var stream = new FileStream(path, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write);
        try
        {
            string sha256;
            await using (stream)
            await using (var progress = output.StartProgress(Resources.ProgressDownloading, file.Info.Size, details: true))
            {
                sha256 = await CopyAsync(file.Content, stream, progress, cancellationToken);
            }

            if (sha256 != file.Info.Sha256)
            {
                throw new ChecksumMismatchException(Resources.DownloadChecksumMismatch);
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

    private static async Task<string> CopyAsync(Stream source, Stream destination, TransferProgress progress, CancellationToken cancellationToken)
    {
        var buffer = new byte[BufferSize];
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var copied = 0L;
        int read;
        while ((read = await source.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            hash.AppendData(buffer, 0, read);
            copied += read;
            progress.Report(copied);
        }

        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }

    private static string ResolvePath(FileInfo? target, string fileName)
    {
        if (target is null)
        {
            return fileName;
        }

        return Directory.Exists(target.FullName) ? Path.Combine(target.FullName, fileName) : target.ToString();
    }
}
