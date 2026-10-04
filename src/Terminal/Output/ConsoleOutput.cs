using System.Globalization;

namespace Mayfly.Terminal.Output;

internal sealed class ConsoleOutput
{
    private const string Green = "\e[32m";
    private const string Red = "\e[31m";
    private const string Dim = "\e[2m";
    private const string Reset = "\e[0m";

    private static readonly string[] SizeUnits = ["B", "KB", "MB", "GB", "TB"];

    private readonly bool _noColor = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NO_COLOR"));

    public bool IsRedirected => Console.IsOutputRedirected;

    public void Value(string value) => Console.Out.WriteLine(value);

    public void NewLine() => Console.Out.WriteLine();

    public void Success(string message) =>
        Console.Out.WriteLine($"{Paint("✓", Green, Console.IsOutputRedirected)} {message}");

    public void Fields(params (string Label, string Value)[] fields)
    {
        var width = fields.Max(field => field.Label.Length);
        foreach (var (label, value) in fields)
        {
            Console.Out.WriteLine($"  {Paint(label.PadRight(width), Dim, Console.IsOutputRedirected)}  {value}");
        }
    }

    public void Error(string message, IEnumerable<string>? details = null)
    {
        Console.Error.WriteLine($"{Paint("✗", Red, Console.IsErrorRedirected)} {message}");
        foreach (var detail in details ?? [])
        {
            Console.Error.WriteLine($"  {detail}");
        }
    }

    public static string FormatSize(long bytes)
    {
        var value = (double)bytes;
        var unit = 0;
        while (value >= 1024 && unit < SizeUnits.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return unit == 0
            ? $"{bytes} B"
            : $"{value.ToString("0.#", CultureInfo.InvariantCulture)} {SizeUnits[unit]}";
    }

    private string Paint(string text, string color, bool redirected) =>
        _noColor || redirected ? text : $"{color}{text}{Reset}";
}
