using System.Security.Cryptography;
using Server.Domain;
using Server.Domain.Abstractions;
using Server.Domain.ValueObjects;

namespace Server.Application.SaveFile;

internal sealed class SaveFileHandler(
    ISharedFileRepository repository,
    IFileStorage storage,
    IPasswordHasher passwordHasher) : ISaveFileHandler
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
            }
            catch
            {
                await repository.DeleteAsync(id, CancellationToken.None);
                throw;
            }

            return new SaveFileResult(id, sha256);
        }

        throw new InvalidOperationException("Could not generate a free share id.");
    }
}
