using Server.Domain.Abstractions;

namespace Server.Application.GetFile;

internal sealed class GetFileHandler(ISharedFileRepository repository, IFileStorage storage) : IGetFileHandler
{
    public async Task<GetFileResult?> HandleAsync(GetFileQuery query, CancellationToken cancellationToken)
    {
        var file = await repository.GetAsync(query.Id, cancellationToken);
        if (file is null || file.IsExpired())
        {
            return null;
        }

        if (!await storage.ExistsAsync(query.Id, cancellationToken))
        {
            return null;
        }

        return new GetFileResult(
            file.FileName,
            file.ContentType,
            file.Size,
            file.ExpiresAt,
            (destination, ct) => storage.CopyToAsync(query.Id, destination, ct));
    }
}
