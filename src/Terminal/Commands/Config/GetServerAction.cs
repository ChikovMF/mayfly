using Mayfly.Terminal.Configuration;
using Mayfly.Terminal.Output;
using Mayfly.Terminal.Properties;
using Microsoft.Extensions.Configuration;

namespace Mayfly.Terminal.Commands.Config;

internal sealed class GetServerAction(IConfiguration configuration, ConsoleOutput output)
{
    public int Execute()
    {
        var server = configuration[ConfigFile.ServerKey];
        if (string.IsNullOrEmpty(server))
        {
            output.Error(Resources.ConfigServerNotSaved);
            return 1;
        }

        output.Value(server);
        return 0;
    }
}
