using Microsoft.Extensions.Logging;
using Server.Domain.Abstractions;
using Server.Domain.ValueObjects;

namespace Server.Application.GetFile;

internal sealed partial class GetFileHandler(
    ISharedFileRepository repository,
    IFileStorage storage,
    IPasswordHasher passwordHasher,
    ILogger<GetFileHandler> logger) : IGetFileHandler
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
            LogContentMissing(query.Id);
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
            file.Sha256,
            (destination, ct) => storage.CopyToAsync(query.Id, destination, ct));
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "File {Id} has a row but no stored content")]
    private partial void LogContentMissing(ShareId id);
}
