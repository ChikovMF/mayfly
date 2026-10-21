using Server.Domain.ValueObjects;

namespace Server.Application.SaveFile;

public sealed record SaveFileCommand(
    FileName FileName,
    ContentType ContentType,
    Stream Content,
    FileSize Size,
    DateTimeOffset? ExpiresAt = null,
    Password? Password = null,
    DownloadsLeft? DownloadsLeft = null,
    Sha256? ExpectedSha256 = null);
