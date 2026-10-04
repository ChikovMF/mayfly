using Mayfly.Terminal;
using Mayfly.Terminal.Commands;
using Mayfly.Terminal.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
    .AddJsonFile(ConfigFile.FilePath, optional: true)
    .Build();

await using var services = new ServiceCollection()
    .AddTerminal(configuration)
    .BuildServiceProvider();

return await services.GetRequiredService<MayflyRootCommand>().RunAsync(args);
