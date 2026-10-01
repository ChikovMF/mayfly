using Server.Domain.Abstractions;

namespace Server.Application;

public sealed class GetFileHandler(ISharedFileRepository repository, IFileStorage storage)
{
    /// <returns>The file with its content, or <see langword="null"/> if the file does not exist.</returns>
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
