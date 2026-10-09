namespace Server.Application.DeleteExpiredFiles;

public interface IDeleteExpiredFilesHandler
{
    Task HandleAsync(DeleteExpiredFilesCommand command, CancellationToken cancellationToken);
}
