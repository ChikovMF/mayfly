using System.Diagnostics.CodeAnalysis;

namespace Server.Domain.ValueObjects;

public sealed record Sha256
{
    private const int ByteLength = 32;

    private Sha256(string hex) => Hex = hex;

    /// <summary>The hash as lowercase hexadecimal characters.</summary>
    public string Hex { get; }

    public byte[] ToBytes() => Convert.FromHexString(Hex);

    /// <exception cref="FormatException"><paramref name="bytes"/> is not a SHA-256 hash.</exception>
    public static Sha256 Parse(ReadOnlySpan<byte> bytes) =>
        TryParse(bytes, out var result)
            ? result
            : throw new FormatException("The bytes are not a SHA-256 hash.");

    public static bool TryParse(ReadOnlySpan<byte> bytes, [MaybeNullWhen(false)] out Sha256 result)
    {
        if (bytes.Length != ByteLength)
        {
            result = null;
            return false;
        }

        result = new Sha256(Convert.ToHexStringLower(bytes));
        return true;
    }
}
