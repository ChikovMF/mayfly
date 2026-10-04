using Mayfly.Terminal.Configuration;
using Mayfly.Terminal.Output;

namespace Mayfly.Terminal.Commands.Config;

internal sealed class SetServerAction(ConsoleOutput output)
{
    public int Execute(Uri server)
    {
        ConfigFile.SaveServer(server);

        output.Success($"Server set to {server}");
        return 0;
    }
}
