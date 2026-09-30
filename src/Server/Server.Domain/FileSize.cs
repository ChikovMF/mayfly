using System.Diagnostics.CodeAnalysis;

namespace Server.Domain;

public sealed record FileSize
{
    private FileSize(long bytes) => Bytes = bytes;

    public long Bytes { get; }

    public static FileSize Parse(long bytes) =>
        TryParse(bytes, out var result)
            ? result
            : throw new ArgumentOutOfRangeException(nameof(bytes), bytes, "File size cannot be negative.");

    public static bool TryParse(long bytes, [MaybeNullWhen(false)] out FileSize result)
    {
        if (bytes < 0)
        {
            result = null;
            return false;
        }

        result = new FileSize(bytes);
        return true;
    }
}
