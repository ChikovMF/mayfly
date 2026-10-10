using Server.Domain.ValueObjects;

namespace Server.Application.GetFile;

public abstract record GetFileResult
{
    private GetFileResult()
    {
    }

    public sealed record Found(
        FileName FileName,
        ContentType ContentType,
        FileSize Size,
        DateTimeOffset? ExpiresAt,
        Func<Stream, CancellationToken, Task> CopyContentToAsync) : GetFileResult;

    public sealed record NotFound : GetFileResult;

    /// <summary>The file is protected by a password and the given one does not match.</summary>
    public sealed record AccessDenied : GetFileResult;
}
