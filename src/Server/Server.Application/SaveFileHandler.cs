using Server.Domain;
using Server.Domain.Abstractions;
using Server.Domain.ValueObjects;

namespace Server.Application;

public sealed class SaveFileHandler(ISharedFileRepository repository, IFileStorage storage)
{
    /// <exception cref="ArgumentException">The content stream does not support seeking.</exception>
    public async Task<ShareId> HandleAsync(SaveFileCommand command, CancellationToken cancellationToken)
    {
        if (!command.Content.CanSeek)
        {
            throw new ArgumentException("Content stream must support seeking.", nameof(command));
        }

        var id = ShareId.New();
        var file = new SharedFile(id, command.FileName, command.ContentType, FileSize.Parse(command.Content.Length));

        await repository.AddAsync(file, cancellationToken);

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
}
