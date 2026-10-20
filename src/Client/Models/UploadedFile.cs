namespace Mayfly.Client.Models;

/// <param name="Id">Identifier of the file.</param>
/// <param name="ExpiresAt">The moment after which the file is unavailable, or <see langword="null"/> if it has no expiration.</param>
/// <param name="MaxDownloads">How many times the file can be downloaded, or <see langword="null"/> if the number is not limited.</param>
/// <param name="Sha256">SHA-256 of the content as lowercase hexadecimal.</param>
public sealed record UploadedFile(string Id, DateTimeOffset? ExpiresAt, int? MaxDownloads, string Sha256);
