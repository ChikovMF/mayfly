using System.Security.Cryptography;
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
        if (!command.Content.CanSeek)
        {
            throw new ArgumentException("Content stream must support seeking.", nameof(command));
        }

        var size = FileSize.Parse(command.Content.Length - command.Content.Position);

        var start = command.Content.Position;
        var sha256 = Sha256.Parse(await SHA256.HashDataAsync(command.Content, cancellationToken));
        command.Content.Position = start;

        var passwordHash = command.Password is null ? null : passwordHasher.Hash(command.Password);

        for (var attempt = 0; attempt < MaxIdAttempts; attempt++)
        {
            var id = ShareId.New();
            var file = new SharedFile(
                id, command.FileName, command.ContentType, size, sha256, command.ExpiresAt, passwordHash, command.DownloadsLeft);

            if (!await repository.AddAsync(file, cancellationToken))
            {
                continue;
            }

            try
            {
                await storage.SaveAsync(id, command.Content, cancellationToken);

                if (!await repository.CompleteAsync(id, cancellationToken))
                {
                    throw new InvalidOperationException($"Upload of file {id} was abandoned before it completed.");
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
