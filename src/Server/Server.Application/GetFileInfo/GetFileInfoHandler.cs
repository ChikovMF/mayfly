using Server.Domain.Abstractions;

namespace Server.Application.GetFileInfo;

internal sealed class GetFileInfoHandler(ISharedFileRepository repository, IFileStorage storage) : IGetFileInfoHandler
{
    public async Task<GetFileInfoResult?> HandleAsync(GetFileInfoQuery query, CancellationToken cancellationToken)
    {
        var file = await repository.GetAsync(query.Id, cancellationToken);
        if (file is null || file.IsExpired() || !await storage.ExistsAsync(query.Id, cancellationToken))
        {
            return null;
        }

        return new GetFileInfoResult(file.FileName, file.ContentType, file.Size, file.ExpiresAt);
    }
}
