namespace Server.Application.GetFile;

public interface IGetFileHandler
{
    /// <returns>The file with its content, or <see langword="null"/> if the file does not exist.</returns>
    Task<GetFileResult?> HandleAsync(GetFileQuery query, CancellationToken cancellationToken);
}
