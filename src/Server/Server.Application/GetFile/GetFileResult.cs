using Server.Domain.ValueObjects;

namespace Server.Application.GetFile;

public sealed record GetFileResult(
    FileName FileName,
    ContentType ContentType,
    FileSize Size,
    DateTimeOffset? ExpiresAt,
    Func<Stream, CancellationToken, Task> CopyContentToAsync);
