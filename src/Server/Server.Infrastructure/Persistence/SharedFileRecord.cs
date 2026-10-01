using Server.Domain;
using Server.Domain.ValueObjects;

namespace Server.Infrastructure.Persistence;

internal sealed record SharedFileRecord(string Id, string FileName, string ContentType, long Size)
{
    public static SharedFileRecord From(SharedFile file) => new(
        file.Id.ToString(),
        file.FileName.ToString(),
        file.ContentType.ToString(),
        file.Size.Bytes
    );

    public SharedFile ToDomain() => new(
        ShareId.Parse(Id),
        Domain.ValueObjects.FileName.Parse(FileName),
        Domain.ValueObjects.ContentType.Parse(ContentType),
        FileSize.Parse(Size)
    );
}