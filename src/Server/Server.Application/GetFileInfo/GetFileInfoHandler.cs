using Microsoft.Extensions.Logging;
using Server.Domain.Abstractions;
using Server.Domain.ValueObjects;

namespace Server.Application.GetFileInfo;

internal sealed partial class GetFileInfoHandler(
    ISharedFileRepository repository,
    IFileStorage storage,
    IPasswordHasher passwordHasher,
    ILogger<GetFileInfoHandler> logger) : IGetFileInfoHandler
{
    public async Task<GetFileInfoResult> HandleAsync(GetFileInfoQuery query, CancellationToken cancellationToken)
    {
        var file = await repository.GetAsync(query.Id, cancellationToken);
        if (file is null || file.IsExpired() || file.IsExhausted())
        {
            return new GetFileInfoResult.NotFound();
        }

        if (file.PasswordHash is not null &&
            (query.Password is null || !passwordHasher.Verify(query.Password, file.PasswordHash)))
        {
            return new GetFileInfoResult.AccessDenied();
        }

        if (!await storage.ExistsAsync(query.Id, cancellationToken))
        {
            LogContentMissing(query.Id);
            return new GetFileInfoResult.NotFound();
        }

        return new GetFileInfoResult.Found(file.FileName, file.ContentType, file.Size, file.ExpiresAt, file.DownloadsLeft, file.Sha256);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "File {Id} has a row but no stored content")]
    private partial void LogContentMissing(ShareId id);
}
