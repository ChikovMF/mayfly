using System.Diagnostics;
using System.Globalization;
using System.Text;
using Mayfly.Terminal.Properties;

namespace Mayfly.Terminal.Output;

internal sealed class TransferProgress : IProgress<long>, IAsyncDisposable
{
    private const int MinBarWidth = 10;
    private const int MaxBarWidth = 40;
    private const int MaxLabelWidth = 30;
    private const int DefaultWindowWidth = 80;
    private const double SpeedSmoothingSeconds = 3;
    private const string BarColors = "\e[36;100m";
    private const string SpinnerColor = "\e[36m";
    private const string Reset = "\e[0m";
    private const string ClearLine = "\r\e[K";
    private const string HideCursor = "\e[?25l";
    private const string ShowCursor = "\e[?25h";
    private const string PartialCells = " ▏▎▍▌▋▊▉";
    private const string SpinnerFrames = "⠋⠙⠹⠸⠼⠴⠦⠧⠇⠏";

    private static readonly TimeSpan FrameInterval = TimeSpan.FromMilliseconds(66);
    private static readonly TimeSpan ShowDelay = TimeSpan.FromMilliseconds(400);
    private static readonly TimeSpan TimeLeftDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MaxTimeLeft = TimeSpan.FromHours(99);

    private readonly string _label;
    private readonly long _total;
    private readonly string? _waitingText;
    private readonly bool _colors;
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly PeriodicTimer _timer = new(FrameInterval);
    private readonly Task _rendering;

    private long _transferred;
    private bool _visible;
    private int _frame;
    private long _sampledBytes;
    private TimeSpan _sampledAt;
    private double _speed;

    public TransferProgress(string label, long total, string? waitingText, bool colors, bool enabled)
    {
        _label = label.Length > MaxLabelWidth ? $"{label[..(MaxLabelWidth - 1)]}…" : label;
        _total = total;
        _waitingText = waitingText;
        _colors = colors;
        _rendering = enabled ? Task.Run(RenderAsync) : Task.CompletedTask;
    }

    public void Report(long value) => Interlocked.Exchange(ref _transferred, value);

    public async ValueTask DisposeAsync()
    {
        _timer.Dispose();
        await _rendering;

        if (_visible)
        {
            Console.Error.Write(ClearLine + ShowCursor);
        }
    }

    private async Task RenderAsync()
    {
        while (await _timer.WaitForNextTickAsync())
        {
            var transferred = Interlocked.Read(ref _transferred);
            UpdateSpeed(transferred);

            if (_clock.Elapsed >= ShowDelay)
            {
                Draw(transferred);
            }
        }
    }

    private void UpdateSpeed(long transferred)
    {
        var now = _clock.Elapsed;
        var seconds = (now - _sampledAt).TotalSeconds;
        var current = (transferred - _sampledBytes) / seconds;

        _speed = _speed > 0
            ? _speed + ((current - _speed) * (1 - Math.Exp(-seconds / SpeedSmoothingSeconds)))
            : current;

        _sampledAt = now;
        _sampledBytes = transferred;
    }

    private void Draw(long transferred)
    {
        var width = WindowWidth() - 1;
        var line = transferred >= _total && _waitingText is not null
            ? Waiting(width)
            : Bar(transferred, width);

        Console.Error.Write($"{(_visible ? "" : HideCursor)}\r{line}\e[K");
        _visible = true;
        _frame++;
    }

    private string Waiting(int width)
    {
        var spinner = SpinnerFrames[_frame % SpinnerFrames.Length].ToString();
        var text = $"{_label}  {spinner} {_waitingText}";
        if (text.Length > width)
        {
            return text[..Math.Max(width, 0)];
        }

        return _colors ? $"{_label}  {SpinnerColor}{spinner}{Reset} {_waitingText}" : text;
    }

    private string Bar(long transferred, int width)
    {
        var fraction = _total == 0 ? 1 : Math.Min(1, (double)transferred / _total);
        var percent = $"{(int)(fraction * 100),3}%";

        var details = new List<string>
        {
            $"{ConsoleOutput.FormatSize(transferred),9} / {ConsoleOutput.FormatSize(_total)}",
            string.Format(CultureInfo.CurrentCulture, Resources.ProgressSpeed, ConsoleOutput.FormatSize((long)_speed)).PadLeft(11),
            TimeLeft(transferred),
        };
        string? label = _label;

        int BarSpace() => width - percent.Length - 1
                          - (label is null ? 0 : label.Length + 2)
                          - details.Sum(detail => detail.Length + 2);

        while (BarSpace() < MinBarWidth && (details.Count > 0 || label is not null))
        {
            if (details.Count > 0)
            {
                details.RemoveAt(details.Count - 1);
            }
            else
            {
                label = null;
            }
        }

        var line = new StringBuilder();
        if (label is not null)
        {
            line.Append(label).Append("  ");
        }

        AppendBar(line, fraction, Math.Clamp(BarSpace(), 0, MaxBarWidth));
        line.Append(' ').Append(percent);
        foreach (var detail in details)
        {
            line.Append("  ").Append(detail);
        }

        return line.ToString();
    }

    private void AppendBar(StringBuilder line, double fraction, int width)
    {
        var cells = fraction * width;
        var full = (int)cells;
        var empty = _colors ? ' ' : '░';

        if (_colors)
        {
            line.Append(BarColors);
        }

        line.Append('█', full);
        if (full < width)
        {
            var partial = (int)((cells - full) * PartialCells.Length);
            line.Append(partial == 0 ? empty : PartialCells[partial]);
            line.Append(empty, width - full - 1);
        }

        if (_colors)
        {
            line.Append(Reset);
        }
    }

    private string TimeLeft(long transferred)
    {
        const int width = 9;
        if (_clock.Elapsed < TimeLeftDelay || _speed <= 0 || transferred >= _total)
        {
            return new string(' ', width);
        }

        var seconds = Math.Ceiling((_total - transferred) / _speed);
        if (seconds > MaxTimeLeft.TotalSeconds)
        {
            return new string(' ', width);
        }

        var left = TimeSpan.FromSeconds(seconds);
        var formatted = left.TotalHours >= 1
            ? string.Create(CultureInfo.InvariantCulture, $"{(int)left.TotalHours}:{left.Minutes:00}:{left.Seconds:00}")
            : string.Create(CultureInfo.InvariantCulture, $"{left.Minutes}:{left.Seconds:00}");

        return string.Format(CultureInfo.CurrentCulture, Resources.ProgressTimeLeft, formatted).PadRight(width);
    }

    private static int WindowWidth()
    {
        try
        {
            return Console.WindowWidth > 0 ? Console.WindowWidth : DefaultWindowWidth;
        }
        catch (IOException)
        {
            return DefaultWindowWidth;
        }
    }
}
