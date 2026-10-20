using System.Globalization;
using System.IO.Compression;
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

        await using var source = directory is null
            ? ((FileInfo)path).OpenRead()
            : await ArchiveAsync(directory, name, cancellationToken);
        var size = source.Length;

        UploadedFile uploaded;
        await using (var progress = output.StartProgress(name, size, Resources.ProgressWaitingForServer))
        await using (var content = new ProgressStream(source, progress))
        {
            uploaded = await client.UploadAsync(
                content,
                name,
                expiresAt: expiresAt,
                password: password,
                maxDownloads: maxDownloads,
                cancellationToken: cancellationToken);
        }

        if (ConsoleOutput.IsRedirected)
        {
            ConsoleOutput.Value(uploaded.Id);
            return 0;
        }

        output.Success(string.Format(CultureInfo.CurrentCulture, Resources.UploadCompleted, name, ConsoleOutput.FormatSize(size)));
        ConsoleOutput.NewLine();

        var fields = new List<(string Label, string Value)> { (Resources.UploadIdLabel, output.Highlight(uploaded.Id)) };
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

    private async Task<FileStream> ArchiveAsync(DirectoryInfo directory, string name, CancellationToken cancellationToken)
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
            await using (output.StartProgress(name, 0, Resources.ProgressArchiving))
            {
                await ZipFile.CreateFromDirectoryAsync(
                    directory.FullName,
                    archive,
                    CompressionLevel.Optimal,
                    includeBaseDirectory: false,
                    cancellationToken);
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
