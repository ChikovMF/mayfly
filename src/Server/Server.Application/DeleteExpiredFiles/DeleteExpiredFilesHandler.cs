using Microsoft.Extensions.Logging;
using Server.Domain.Abstractions;
using Server.Domain.ValueObjects;

namespace Server.Application.DeleteExpiredFiles;

internal sealed partial class DeleteExpiredFilesHandler(
    ISharedFileRepository repository,
    IFileStorage storage,
    ILogger<DeleteExpiredFilesHandler> logger) : IDeleteExpiredFilesHandler
{
    private const int BatchSize = 100;

    public async Task HandleAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var ids = await repository.GetExpiredIdsAsync(BatchSize, cancellationToken);
            if (ids.Count == 0)
            {
                return;
            }

            foreach (var id in ids)
            {
                await storage.DeleteAsync(id, cancellationToken);
                await repository.DeleteAsync(id, cancellationToken);
                LogFileDeleted(id);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "File {Id} deleted: expiration time has passed")]
    private partial void LogFileDeleted(ShareId id);
}
