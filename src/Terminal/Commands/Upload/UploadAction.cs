using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using Mayfly.Client;
using Mayfly.Client.Models;
using Mayfly.Terminal.Input;
using Mayfly.Terminal.Output;
using Mayfly.Terminal.Properties;

namespace Mayfly.Terminal.Commands.Upload;

internal sealed class UploadAction(IMayflyClient client, ConsoleOutput output)
{
    public async Task<int> ExecuteAsync(
        FileSystemInfo path,
        TimeSpan? expiresIn,
        int? maxDownloads,
        bool protect,
        CancellationToken cancellationToken)
    {
        var directory = path as DirectoryInfo;
        var name = directory is null ? path.Name : $"{directory.Name}.zip";
        if (directory is not null)
        {
            if (!directory.EnumerateFileSystemInfos().Any())
            {
                output.Error(string.Format(CultureInfo.CurrentCulture, Resources.UploadDirectoryEmpty, directory));
                return 1;
            }

            output.Info(string.Format(CultureInfo.CurrentCulture, Resources.UploadDirectoryArchived, directory, name));
        }

        string? password = null;
        if (protect)
        {
            password = ReadNewPassword();
            if (password is null)
            {
                return 1;
            }
        }

        DateTimeOffset? expiresAt = null;
        if (expiresIn.HasValue)
        {
            expiresAt = DateTimeOffset.UtcNow + expiresIn.Value;
        }

        var steps = directory is null ? 2 : 3;
        await using var source = directory is null
            ? ((FileInfo)path).OpenRead()
            : await ArchiveAsync(directory, Step(1, steps, Resources.ProgressArchiving), cancellationToken);
        var size = source.Length;

        string sha256;
        await using (var hashing = output.StartProgress(Step(steps - 1, steps, Resources.ProgressHashing), size, details: false))
        {
            sha256 = Convert.ToHexStringLower(await SHA256.HashDataAsync(new ProgressStream(source, hashing), cancellationToken));
        }

        source.Position = 0;

        UploadedFile uploaded;
        await using (var progress = output.StartProgress(Step(steps, steps, Resources.ProgressUploading), size, details: true, Resources.ProgressSavingOnServer))
        await using (var content = new ProgressStream(source, progress))
        {
            uploaded = await client.UploadAsync(
                content,
                name,
                expiresAt: expiresAt,
                password: password,
                maxDownloads: maxDownloads,
                sha256: sha256,
                cancellationToken: cancellationToken);
        }

        if (ConsoleOutput.IsRedirected)
        {
            ConsoleOutput.Value(uploaded.Id);
            return 0;
        }

        output.Success(string.Format(CultureInfo.CurrentCulture, Resources.UploadCompleted, name, ConsoleOutput.FormatSize(size)));
        ConsoleOutput.NewLine();

        var fields = new List<(string Label, string Value)>
        {
            (Resources.UploadIdLabel, output.Highlight(uploaded.Id)),
            (Resources.Sha256Label, uploaded.Sha256),
        };
        if (uploaded.ExpiresAt.HasValue)
        {
            fields.Add((Resources.ExpiresLabel, ConsoleOutput.FormatDate(uploaded.ExpiresAt.Value)));
        }

        if (uploaded.MaxDownloads.HasValue)
        {
            fields.Add((Resources.DownloadsLeftLabel, uploaded.MaxDownloads.Value.ToString(CultureInfo.CurrentCulture)));
        }

        output.Fields([.. fields]);
        return 0;
    }

    private static string Step(int number, int count, string label) => $"[{number}/{count}] {label}";

    private static DateTime ZipTimestamp(DateTime lastWriteTime) =>
        lastWriteTime.Year is < 1980 or > 2107 ? new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Local) : lastWriteTime;

    private async Task<FileStream> ArchiveAsync(DirectoryInfo directory, string label, CancellationToken cancellationToken)
    {
        var archive = new FileStream(
            Path.GetTempFileName(),
            FileMode.Create,
            FileAccess.ReadWrite,
            FileShare.None,
            bufferSize: 4096,
            FileOptions.DeleteOnClose);

        try
        {
            var entries = directory.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).ToList();
            var total = entries.OfType<FileInfo>().Sum(file => file.Length);

            await using (var progress = output.StartProgress(label, total, details: false))
            await using (var zip = await ZipArchive.CreateAsync(archive, ZipArchiveMode.Create, leaveOpen: true, entryNameEncoding: null, cancellationToken))
            {
                var buffer = new byte[81920];
                long archived = 0;
                foreach (var entry in entries)
                {
                    var entryName = Path.GetRelativePath(directory.FullName, entry.FullName).Replace(Path.DirectorySeparatorChar, '/');
                    if (entry is DirectoryInfo subdirectory)
                    {
                        if (!subdirectory.EnumerateFileSystemInfos().Any())
                        {
                            zip.CreateEntry($"{entryName}/");
                        }

                        continue;
                    }

                    var zipEntry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
                    zipEntry.LastWriteTime = ZipTimestamp(entry.LastWriteTime);

                    await using var source = ((FileInfo)entry).OpenRead();
                    await using var target = await zipEntry.OpenAsync(cancellationToken);
                    int read;
                    while ((read = await source.ReadAsync(buffer, cancellationToken)) > 0)
                    {
                        await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                        archived += read;
                        progress.Report(archived);
                    }
                }
            }

            archive.Position = 0;
            return archive;
        }
        catch
        {
            await archive.DisposeAsync();
            throw;
        }
    }

    private string? ReadNewPassword()
    {
        if (!PasswordPrompt.IsAvailable)
        {
            output.Error(Resources.PasswordPromptUnavailable);
            return null;
        }

        var password = PasswordPrompt.Read(Resources.PasswordPromptLabel);
        if (password.Length == 0)
        {
            output.Error(Resources.PasswordEmpty);
            return null;
        }

        if (PasswordPrompt.Read(Resources.PasswordPromptRepeatLabel) != password)
        {
            output.Error(Resources.PasswordsDoNotMatch);
            return null;
        }

        return password;
    }
}
