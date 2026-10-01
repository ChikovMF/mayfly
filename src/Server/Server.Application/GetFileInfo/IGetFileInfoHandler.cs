namespace Server.Application.GetFileInfo;

public interface IGetFileInfoHandler
{
    /// <returns>The file information, or <see langword="null"/> if the file does not exist.</returns>
    Task<GetFileInfoResult?> HandleAsync(GetFileInfoQuery query, CancellationToken cancellationToken);
}
