using Server.Domain.Abstractions;

namespace Server.Application.GetFileInfo;

internal sealed class GetFileInfoHandler(ISharedFileRepository repository) : IGetFileInfoHandler
{
    public async Task<GetFileInfoResult?> HandleAsync(GetFileInfoQuery query, CancellationToken cancellationToken)
    {
        var file = await repository.GetAsync(query.Id, cancellationToken);
        return file is null ? null : new GetFileInfoResult(file.FileName, file.ContentType, file.Size);
    }
}
