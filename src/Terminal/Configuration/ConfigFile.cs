using System.Text.Json;
using System.Text.Json.Nodes;

namespace Mayfly.Terminal.Configuration;

internal static class ConfigFile
{
    public const string ServerKey = "Mayfly:BaseAddress";

    public static string FilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify),
        "mayfly",
        "config.json");

    public static void SaveServer(Uri server)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

        using var stream = File.Create(FilePath);
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

        var settings = new JsonObject { ["Mayfly"] = new JsonObject { ["BaseAddress"] = server.ToString() } };
        settings.WriteTo(writer);
    }
}
