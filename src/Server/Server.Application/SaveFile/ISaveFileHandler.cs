namespace Server.Application.SaveFile;

public interface ISaveFileHandler
{
    /// <exception cref="ArgumentException">The content is not exactly <see cref="SaveFileCommand.Size"/> bytes long.</exception>
    /// <exception cref="UploadAbandonedException">The upload was abandoned before it completed.</exception>
    Task<SaveFileResult> HandleAsync(SaveFileCommand command, CancellationToken cancellationToken);
}
