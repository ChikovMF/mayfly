using Server.Domain.ValueObjects;

namespace Server.Application.SaveFile;

public interface ISaveFileHandler
{
    /// <exception cref="ArgumentException">The content stream does not support seeking.</exception>
    Task<ShareId> HandleAsync(SaveFileCommand command, CancellationToken cancellationToken);
}
