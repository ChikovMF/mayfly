using System.Globalization;

namespace Mayfly.Terminal;

internal static class Output
{
    private const string Green = "\e[32m";
    private const string Red = "\e[31m";
    private const string Dim = "\e[2m";
    private const string Reset = "\e[0m";

    private static readonly bool NoColor = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NO_COLOR"));

    public static bool IsRedirected => Console.IsOutputRedirected;

    public static void Success(string message) =>
        Console.Out.WriteLine($"{Paint("✓", Green, Console.IsOutputRedirected)} {message}");

    public static void Fields(params (string Label, string Value)[] fields)
    {
        var width = fields.Max(field => field.Label.Length);
        foreach (var (label, value) in fields)
        {
            Console.Out.WriteLine($"  {Paint(label.PadRight(width), Dim, Console.IsOutputRedirected)}  {value}");
        }
    }

    public static void Error(string message, IEnumerable<string>? details = null)
    {
        Console.Error.WriteLine($"{Paint("✗", Red, Console.IsErrorRedirected)} {message}");
        foreach (var detail in details ?? [])
        {
            Console.Error.WriteLine($"  {detail}");
        }
    }

    public static string Size(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        var value = (double)bytes;
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return unit == 0
            ? $"{bytes} B"
            : $"{value.ToString("0.#", CultureInfo.InvariantCulture)} {units[unit]}";
    }

    private static string Paint(string text, string color, bool redirected) =>
        NoColor || redirected ? text : $"{color}{text}{Reset}";
}
