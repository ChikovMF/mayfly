namespace Server.Application.DeleteExpiredFiles;

public interface IDeleteExpiredFilesHandler
{
    Task HandleAsync(CancellationToken cancellationToken);
}
