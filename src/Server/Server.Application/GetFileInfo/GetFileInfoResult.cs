using Server.Domain.ValueObjects;

namespace Server.Application.GetFileInfo;

public abstract record GetFileInfoResult
{
    private GetFileInfoResult()
    {
    }

    public sealed record Found(
        FileName FileName,
        ContentType ContentType,
        FileSize Size,
        DateTimeOffset? ExpiresAt) : GetFileInfoResult;

    public sealed record NotFound : GetFileInfoResult;

    public sealed record AccessDenied : GetFileInfoResult;
}
