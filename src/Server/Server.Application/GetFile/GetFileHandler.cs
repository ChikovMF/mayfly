using Server.Domain.Abstractions;

namespace Server.Application.GetFile;

internal sealed class GetFileHandler(ISharedFileRepository repository, IFileStorage storage) : IGetFileHandler
{
    public async Task<GetFileResult?> HandleAsync(GetFileQuery query, CancellationToken cancellationToken)
    {
        var file = await repository.GetAsync(query.Id, cancellationToken);
        if (file is null)
        {
            return null;
        }

        var content = await storage.OpenReadAsync(query.Id, cancellationToken);
        return content is null ? null : new GetFileResult(file.FileName, file.ContentType, content);
    }
}
