using Microsoft.Extensions.Logging;
using Server.Domain;
using Server.Domain.Abstractions;
using Server.Domain.ValueObjects;

namespace Server.Application.SaveFile;

internal sealed partial class SaveFileHandler(
    ISharedFileRepository repository,
    IFileStorage storage,
    IPasswordHasher passwordHasher,
    ILogger<SaveFileHandler> logger) : ISaveFileHandler
{
    private const int MaxIdAttempts = 5;

    public async Task<SaveFileResult> HandleAsync(SaveFileCommand command, CancellationToken cancellationToken)
    {
        await using var content = new HashingStream(command.Content);

        var passwordHash = command.Password is null ? null : passwordHasher.Hash(command.Password);

        for (var attempt = 0; attempt < MaxIdAttempts; attempt++)
        {
            var id = ShareId.New();
            var file = new PendingFile(
                id, command.FileName, command.ContentType, command.Size, command.ExpiresAt, passwordHash, command.DownloadsLeft);

            if (!await repository.AddAsync(file, cancellationToken))
            {
                continue;
            }

            Sha256 sha256;
            try
            {
                await storage.SaveAsync(id, content, command.Size, cancellationToken);

                if (content.BytesRead != command.Size.Bytes)
                {
                    throw new ArgumentException($"The content is {content.BytesRead} bytes, but the command declares {command.Size.Bytes}.", nameof(command));
                }

                sha256 = Sha256.Parse(content.GetHash());
                if (!await repository.CompleteAsync(id, sha256, cancellationToken))
                {
                    throw new UploadAbandonedException(id);
                }
            }
            catch
            {
                try
                {
                    await storage.DeleteAsync(id, CancellationToken.None);
                    await repository.DeleteAsync(id, CancellationToken.None);
                }
                catch (Exception exception)
                {
                    LogRollbackFailed(exception, id);
                }

                throw;
            }

            LogFileSaved(id);
            return new SaveFileResult(id, sha256);
        }

        throw new InvalidOperationException("Could not generate a free share id.");
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "File {Id} saved")]
    private partial void LogFileSaved(ShareId id);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to delete file {Id} after its upload failed, it will be deleted as an abandoned upload")]
    private partial void LogRollbackFailed(Exception exception, ShareId id);
}
