using Server.Domain.ValueObjects;

namespace Server.Domain;

public sealed class SharedFile(
    ShareId id,
    FileName fileName,
    ContentType contentType,
    FileSize size,
    DateTimeOffset? expiresAt = null,
    string? passwordHash = null)
{
    public ShareId Id { get; } = id;

    public FileName FileName { get; } = fileName;

    public ContentType ContentType { get; } = contentType;

    public FileSize Size { get; } = size;

    public DateTimeOffset? ExpiresAt { get; } = expiresAt;

    public string? PasswordHash { get; } = passwordHash;

    public bool IsExpired() => ExpiresAt <= DateTimeOffset.UtcNow;
}
