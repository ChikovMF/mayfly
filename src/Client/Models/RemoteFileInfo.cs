namespace Mayfly.Client.Models;

/// <param name="FileName">Name of the file.</param>
/// <param name="ContentType">MIME type of the file.</param>
/// <param name="Size">Size of the file in bytes.</param>
/// <param name="ExpiresAt">The moment after which the file is unavailable, or <see langword="null"/> if it has no expiration.</param>
/// <param name="DownloadsLeft">How many times the file can still be downloaded, or <see langword="null"/> if the number is not limited. For a downloaded file it excludes that download.</param>
/// <param name="Sha256">SHA-256 of the content as lowercase hexadecimal.</param>
public sealed record RemoteFileInfo(
    string FileName,
    string ContentType,
    long Size,
    DateTimeOffset? ExpiresAt,
    int? DownloadsLeft,
    string Sha256);
