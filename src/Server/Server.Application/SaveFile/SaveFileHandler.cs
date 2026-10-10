using Server.Domain;
using Server.Domain.Abstractions;
using Server.Domain.ValueObjects;

namespace Server.Application.SaveFile;

internal sealed class SaveFileHandler(ISharedFileRepository repository, IFileStorage storage) : ISaveFileHandler
{
    private const int MaxIdAttempts = 5;

    public async Task<ShareId> HandleAsync(SaveFileCommand command, CancellationToken cancellationToken)
    {
        if (!command.Content.CanSeek)
        {
            throw new ArgumentException("Content stream must support seeking.", nameof(command));
        }

        var size = FileSize.Parse(command.Content.Length - command.Content.Position);

        for (var attempt = 0; attempt < MaxIdAttempts; attempt++)
        {
            var id = ShareId.New();
            var file = new SharedFile(id, command.FileName, command.ContentType, size, command.ExpiresAt);

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

            return id;
        }

        throw new InvalidOperationException("Could not generate a free share id.");
    }
}
