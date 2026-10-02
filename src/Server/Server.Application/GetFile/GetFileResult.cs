using Server.Domain.ValueObjects;

namespace Server.Application.GetFile;

public sealed record GetFileResult(
    FileName FileName,
    ContentType ContentType,
    FileSize Size,
    Func<Stream, CancellationToken, Task> CopyContentToAsync);
