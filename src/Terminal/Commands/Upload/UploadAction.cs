using System.Globalization;
using Mayfly.Client;
using Mayfly.Client.Models;
using Mayfly.Terminal.Input;
using Mayfly.Terminal.Output;
using Mayfly.Terminal.Properties;

namespace Mayfly.Terminal.Commands.Upload;

internal sealed class UploadAction(IMayflyClient client, ConsoleOutput output)
{
    public async Task<int> ExecuteAsync(
        FileInfo file,
        TimeSpan? expiresIn,
        bool protect,
        CancellationToken cancellationToken)
    {
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

        UploadedFile uploaded;
        await using (var progress = output.StartProgress(file.Name, file.Length, Resources.ProgressWaitingForServer))
        await using (var content = new ProgressStream(file.OpenRead(), progress))
        {
            uploaded = await client.UploadAsync(
                content,
                file.Name,
                expiresAt: expiresAt,
                password: password,
                cancellationToken: cancellationToken);
        }

        if (ConsoleOutput.IsRedirected)
        {
            ConsoleOutput.Value(uploaded.Id);
            return 0;
        }

        output.Success(string.Format(CultureInfo.CurrentCulture, Resources.UploadCompleted, file.Name, ConsoleOutput.FormatSize(file.Length)));
        ConsoleOutput.NewLine();

        var fields = new List<(string Label, string Value)> { (Resources.UploadIdLabel, output.Highlight(uploaded.Id)) };
        if (uploaded.ExpiresAt.HasValue)
        {
            fields.Add((Resources.ExpiresLabel, ConsoleOutput.FormatDate(uploaded.ExpiresAt.Value)));
        }

        output.Fields([.. fields]);
        return 0;
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
