using System.Globalization;
using Mayfly.Client;
using Mayfly.Terminal.Output;
using Mayfly.Terminal.Properties;

namespace Mayfly.Terminal.Commands.Info;

internal sealed class InfoAction(IMayflyClient client, ConsoleOutput output)
{
    public async Task<int> ExecuteAsync(string id, CancellationToken cancellationToken)
    {
        var info = await client.GetInfoAsync(id, cancellationToken: cancellationToken);
        if (info is null)
        {
            output.Error(string.Format(CultureInfo.CurrentCulture, Resources.FileNotFound, id));
            return 1;
        }

        var size = ConsoleOutput.FormatSize(info.Size);
        if (info.Size >= 1024)
        {
            size = string.Format(CultureInfo.CurrentCulture, Resources.InfoSizeInBytes, size, info.Size);
        }

        var fields = new List<(string Label, string Value)>
        {
            (Resources.InfoNameLabel, info.FileName),
            (Resources.InfoTypeLabel, info.ContentType),
            (Resources.InfoSizeLabel, size),
        };

        if (info.ExpiresAt.HasValue)
        {
            fields.Add((Resources.ExpiresLabel, ConsoleOutput.FormatDate(info.ExpiresAt.Value)));
        }

        output.Fields([.. fields]);
        return 0;
    }
}
