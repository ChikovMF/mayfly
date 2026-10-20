using Server.Domain.ValueObjects;

namespace Server.Application.GetFile;

public abstract record GetFileResult
{
    private GetFileResult()
    {
    }

    /// <summary>The file is available. <c>DownloadsLeft</c> is what remains after this download, <see langword="null"/> if the number is not limited.</summary>
    public sealed record Found(
        FileName FileName,
        ContentType ContentType,
        FileSize Size,
        DateTimeOffset? ExpiresAt,
        DownloadsLeft? DownloadsLeft,
        Func<Stream, CancellationToken, Task> CopyContentToAsync) : GetFileResult;

    public sealed record NotFound : GetFileResult;

    /// <summary>The file is protected by a password and the given one does not match.</summary>
    public sealed record AccessDenied : GetFileResult;
}
