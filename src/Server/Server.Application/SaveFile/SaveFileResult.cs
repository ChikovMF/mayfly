using Server.Domain.ValueObjects;

namespace Server.Application.SaveFile;

public abstract record SaveFileResult
{
    private SaveFileResult()
    {
    }

    public sealed record Saved(ShareId Id, Sha256 Sha256) : SaveFileResult;

    /// <summary>The content does not match <see cref="SaveFileCommand.ExpectedSha256"/>; nothing is stored.</summary>
    public sealed record ChecksumMismatch : SaveFileResult;
}
