using Server.Domain;

namespace Server.Infrastructure.Persistence;

internal sealed record PendingFileRecord(
    string Id,
    string FileName,
    string ContentType,
    long Size,
    DateTime? ExpiresAt,
    string? PasswordHash,
    int? DownloadsLeft)
{
    public static PendingFileRecord From(PendingFile file) => new(
        file.Id.ToString(),
        file.FileName.ToString(),
        file.ContentType.ToString(),
        file.Size.Bytes,
        file.ExpiresAt?.UtcDateTime,
        file.PasswordHash,
        file.DownloadsLeft?.Count
    );
}
