using System.CommandLine;
using Mayfly.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Mayfly.Terminal.Commands;

internal static class UploadCommand
{
    public static Command Create(ServerOption serverOption)
    {
        var fileArgument = new Argument<FileInfo>("file") { Description = "File to upload." };
        fileArgument.Validators.Add(result =>
        {
            if (result.GetValueOrDefault<FileInfo>() is { Exists: false } file)
            {
                result.AddError($"File not found: '{file}'.");
            }
        });

        var command = new Command("upload", "Upload a file and print its link.") { fileArgument, serverOption };
        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var file = parseResult.GetRequiredValue(fileArgument);
            var server = parseResult.GetRequiredValue(serverOption);

            await using var services = MayflyServices.Create(server);
            var client = services.GetRequiredService<IMayflyClient>();

            await using var content = file.OpenRead();
            var uploaded = await client.UploadAsync(content, file.Name, cancellationToken: cancellationToken);

            var link = new Uri(server, $"api/files/{Uri.EscapeDataString(uploaded.Id)}").ToString();
            if (Output.IsRedirected)
            {
                Console.Out.WriteLine(link);
                return;
            }

            Output.Success($"Uploaded {file.Name} ({Output.Size(file.Length)})");
            Console.Out.WriteLine();
            Output.Fields(("Link", link), ("ID", uploaded.Id));
        });

        return command;
    }
}
