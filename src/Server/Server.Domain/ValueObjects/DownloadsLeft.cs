using System.Diagnostics.CodeAnalysis;

namespace Server.Domain.ValueObjects;

public sealed record DownloadsLeft
{
    private DownloadsLeft(int count) => Count = count;

    public int Count { get; }

    /// <exception cref="FormatException"><paramref name="count"/> is negative.</exception>
    public static DownloadsLeft Parse(int count) =>
        TryParse(count, out var result)
            ? result
            : throw new FormatException($"'{count}' is not a valid number of downloads.");

    public static bool TryParse(int count, [MaybeNullWhen(false)] out DownloadsLeft result)
    {
        if (count < 0)
        {
            result = null;
            return false;
        }

        result = new DownloadsLeft(count);
        return true;
    }
}
