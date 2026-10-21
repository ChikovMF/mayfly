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

    private static readonly TimeSpan FirstRetryDelay = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromDays(1);

    public async Task HandleAsync(DeleteExpiredFilesCommand command, CancellationToken cancellationToken)
    {
        while (true)
        {
            var files = await repository.ClaimExpiredAsync(
                BatchSize,
                command.Lease,
                command.ExhaustedFileDelay,
                command.MaxAttempts,
                cancellationToken);

            if (files.Count == 0)
            {
                return;
            }

            foreach (var file in files)
            {
                await DeleteAsync(file, command.MaxAttempts, cancellationToken);
            }
        }
    }

    private async Task DeleteAsync(ExpiredFileClaim file, int maxAttempts, CancellationToken cancellationToken)
    {
        try
        {
            await storage.DeleteAsync(file.Id, cancellationToken);
            await repository.DeleteAsync(file.Id, cancellationToken);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            var failedAttempts = file.FailedAttempts + 1;
            await repository.PostponeDeletionAsync(file.Id, RetryDelay(failedAttempts), cancellationToken);

            if (failedAttempts >= maxAttempts)
            {
                LogDeletionAbandoned(exception, file.Id, failedAttempts);
            }
            else
            {
                LogDeletionFailed(exception, file.Id, failedAttempts);
            }

            return;
        }

        switch (file.Reason)
        {
            case FileDeletionReason.Expired:
                LogFileExpired(file.Id);
                break;
            case FileDeletionReason.DownloadsExhausted:
                LogFileDownloadsExhausted(file.Id);
                break;
            default:
                LogUploadAbandoned(file.Id);
                break;
        }
    }

    private static TimeSpan RetryDelay(int failedAttempts)
    {
        var minutes = FirstRetryDelay.TotalMinutes * Math.Pow(2, failedAttempts - 1);
        return TimeSpan.FromMinutes(Math.Min(minutes, MaxRetryDelay.TotalMinutes));
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "File {Id} deleted: expiration time has passed")]
    private partial void LogFileExpired(ShareId id);

    [LoggerMessage(Level = LogLevel.Information, Message = "File {Id} deleted: no downloads left")]
    private partial void LogFileDownloadsExhausted(ShareId id);

    [LoggerMessage(Level = LogLevel.Information, Message = "File {Id} deleted: upload was not completed")]
    private partial void LogUploadAbandoned(ShareId id);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to delete file {Id} (attempt {Attempt}), will retry later")]
    private partial void LogDeletionFailed(Exception exception, ShareId id, int attempt);

    [LoggerMessage(
        Level = LogLevel.Critical,
        Message = "Failed to delete file {Id} after {Attempt} attempts, no more retries until the attempts are reset")]
    private partial void LogDeletionAbandoned(Exception exception, ShareId id, int attempt);
}
