using Server.Domain;
using Server.Domain.ValueObjects;

namespace Server.Infrastructure.Persistence;

internal sealed record SharedFileRecord(
    string Id,
    string FileName,
    string ContentType,
    long Size,
    DateTime? ExpiresAt,
    string? PasswordHash)
{
    public static SharedFileRecord From(SharedFile file) => new(
        file.Id.ToString(),
        file.FileName.ToString(),
        file.ContentType.ToString(),
        file.Size.Bytes,
        file.ExpiresAt?.UtcDateTime,
        file.PasswordHash
    );

    public SharedFile ToDomain()
    {
        DateTimeOffset? expiresAt = null;
        if (ExpiresAt.HasValue)
        {
            expiresAt = new DateTimeOffset(DateTime.SpecifyKind(ExpiresAt.Value, DateTimeKind.Utc));
        }

        return new SharedFile(
            ShareId.Parse(Id),
            Domain.ValueObjects.FileName.Parse(FileName),
            Domain.ValueObjects.ContentType.Parse(ContentType),
            FileSize.Parse(Size),
            expiresAt,
            PasswordHash
        );
    }
}