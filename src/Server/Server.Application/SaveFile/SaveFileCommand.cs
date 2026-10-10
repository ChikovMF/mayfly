using Server.Domain.ValueObjects;

namespace Server.Application.SaveFile;

public sealed record SaveFileCommand(
    FileName FileName,
    ContentType ContentType,
    Stream Content,
    DateTimeOffset? ExpiresAt = null,
    Password? Password = null);
