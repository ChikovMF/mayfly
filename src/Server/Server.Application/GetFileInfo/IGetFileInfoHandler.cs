namespace Server.Application.GetFileInfo;

public interface IGetFileInfoHandler
{
    Task<GetFileInfoResult> HandleAsync(GetFileInfoQuery query, CancellationToken cancellationToken);
}
