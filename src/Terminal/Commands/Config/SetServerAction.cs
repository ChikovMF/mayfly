using System.Globalization;
using Mayfly.Terminal.Configuration;
using Mayfly.Terminal.Output;
using Mayfly.Terminal.Properties;

namespace Mayfly.Terminal.Commands.Config;

internal sealed class SetServerAction(ConsoleOutput output)
{
    public int Execute(Uri server)
    {
        ConfigFile.SaveServer(server);

        output.Success(string.Format(CultureInfo.CurrentCulture, Resources.ConfigServerSaved, server));
        return 0;
    }
}
