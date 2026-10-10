namespace Server.Application.GetFile;

public interface IGetFileHandler
{
    Task<GetFileResult> HandleAsync(GetFileQuery query, CancellationToken cancellationToken);
}
