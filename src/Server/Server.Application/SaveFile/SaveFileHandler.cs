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

        var passwordHash = command.Password is null ? null : passwordHasher.Hash(command.Password);

        for (var attempt = 0; attempt < MaxIdAttempts; attempt++)
        {
            var id = ShareId.New();
            var file = new SharedFile(id, command.FileName, command.ContentType, size, command.ExpiresAt, passwordHash, command.DownloadsLeft);

            if (!await repository.AddAsync(file, cancellationToken))
            {
                continue;
            }

            Sha256? sha256;
            try
            {
                using var content = new HashingStream(command.Content);
                await storage.SaveAsync(id, content, cancellationToken);

                sha256 = content.GetHash();
                if (sha256 is not null)
                {
                    await repository.SetSha256Async(id, sha256, cancellationToken);
                }
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
