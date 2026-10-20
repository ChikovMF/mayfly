using Server.Domain.Abstractions;

namespace Server.Application.GetFile;

internal sealed class GetFileHandler(
    ISharedFileRepository repository,
    IFileStorage storage,
    IPasswordHasher passwordHasher) : IGetFileHandler
{
    public async Task<GetFileResult> HandleAsync(GetFileQuery query, CancellationToken cancellationToken)
    {
        var file = await repository.GetAsync(query.Id, cancellationToken);
        if (file is null || file.IsExpired() || file.IsExhausted())
        {
            return new GetFileResult.NotFound();
        }

        if (file.PasswordHash is not null &&
            (query.Password is null || !passwordHasher.Verify(query.Password, file.PasswordHash)))
        {
            return new GetFileResult.AccessDenied();
        }

        if (!await storage.ExistsAsync(query.Id, cancellationToken))
        {
            return new GetFileResult.NotFound();
        }

        var downloadsLeft = file.DownloadsLeft;
        if (downloadsLeft != null)
        {
            downloadsLeft = await repository.SpendDownloadAsync(query.Id, cancellationToken);
            if (downloadsLeft is null)
            {
                return new GetFileResult.NotFound();
            }
        }

        return new GetFileResult.Found(
            file.FileName,
            file.ContentType,
            file.Size,
            file.ExpiresAt,
            downloadsLeft,
            (destination, ct) => storage.CopyToAsync(query.Id, destination, ct));
    }
}
