using System.Globalization;
using Mayfly.Client;
using Mayfly.Terminal.Output;
using Mayfly.Terminal.Properties;

namespace Mayfly.Terminal.Commands.Upload;

internal sealed class UploadAction(IMayflyClient client, ConsoleOutput output)
{
    public async Task<int> ExecuteAsync(FileInfo file, TimeSpan? expiresIn, CancellationToken cancellationToken)
    {
        await using var content = file.OpenRead();

        DateTimeOffset? expiresAt = null;
        if (expiresIn.HasValue)
        {
            expiresAt = DateTimeOffset.UtcNow + expiresIn.Value;
        }

        var uploaded = await client.UploadAsync(content, file.Name, expiresAt: expiresAt, cancellationToken: cancellationToken);

        if (ConsoleOutput.IsRedirected)
        {
            ConsoleOutput.Value(uploaded.Id);
            return 0;
        }

        output.Success(string.Format(CultureInfo.CurrentCulture, Resources.UploadCompleted, file.Name, ConsoleOutput.FormatSize(file.Length)));
        ConsoleOutput.NewLine();

        var fields = new List<(string Label, string Value)> { (Resources.UploadIdLabel, uploaded.Id) };
        if (uploaded.ExpiresAt.HasValue)
        {
            fields.Add((Resources.ExpiresLabel, ConsoleOutput.FormatDate(uploaded.ExpiresAt.Value)));
        }

        output.Fields([.. fields]);
        return 0;
    }
}
