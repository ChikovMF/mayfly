using System.Globalization;
using Mayfly.Terminal.Properties;

namespace Mayfly.Terminal.Output;

internal sealed class ConsoleOutput
{
    private const string Green = "\e[32m";
    private const string Red = "\e[31m";
    private const string Dim = "\e[2m";
    private const string BoldCyan = "\e[1;36m";
    private const string Reset = "\e[0m";

    private readonly bool _noColor = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NO_COLOR"));

    public static bool IsRedirected => Console.IsOutputRedirected;

    public static void Value(string value) => Console.Out.WriteLine(value);

    public static void NewLine() => Console.Out.WriteLine();

    public void Success(string message) =>
        Console.Out.WriteLine($"{Paint("✓", Green, Console.IsOutputRedirected)} {message}");

    public string Highlight(string text) => Paint(text, BoldCyan, Console.IsOutputRedirected);

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

    public TransferProgress StartProgress(string label, long total, string? waitingText = null) =>
        new(
            label,
            total,
            waitingText,
            colors: !_noColor,
            enabled: !Console.IsErrorRedirected && Environment.GetEnvironmentVariable("TERM") != "dumb");

    public static string FormatSize(long bytes)
    {
        string[] units =
        [
            Resources.SizeUnitBytes,
            Resources.SizeUnitKilobytes,
            Resources.SizeUnitMegabytes,
            Resources.SizeUnitGigabytes,
            Resources.SizeUnitTerabytes,
        ];

        var value = (double)bytes;
        var unit = 0;
        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return $"{value.ToString(unit == 0 ? "0" : "0.#", CultureInfo.CurrentCulture)} {units[unit]}";
    }

    public static string FormatDate(DateTimeOffset value) =>
        value.ToLocalTime().ToString("g", CultureInfo.CurrentCulture);

    private string Paint(string text, string color, bool redirected) =>
        _noColor || redirected ? text : $"{color}{text}{Reset}";
}
