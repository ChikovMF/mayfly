using Server.Domain.ValueObjects;

namespace Server.Domain;

/// <summary>A file whose upload is still in progress.</summary>
public sealed record PendingFile(
    ShareId Id,
    FileName FileName,
    ContentType ContentType,
    FileSize Size,
    DateTimeOffset? ExpiresAt,
    string? PasswordHash,
    DownloadsLeft? DownloadsLeft);
