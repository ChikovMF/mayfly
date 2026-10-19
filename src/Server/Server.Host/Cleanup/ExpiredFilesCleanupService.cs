using Microsoft.Extensions.Options;
using Server.Application.DeleteExpiredFiles;

namespace Server.Host.Cleanup;

internal sealed partial class ExpiredFilesCleanupService(
    IServiceScopeFactory scopeFactory,
    IOptions<CleanupOptions> options,
    ILogger<ExpiredFilesCleanupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.Interval);
        var command = new DeleteExpiredFilesCommand(options.Value.Lease, options.Value.MaxAttempts);

        do
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var handler = scope.ServiceProvider.GetRequiredService<IDeleteExpiredFilesHandler>();
                await handler.HandleAsync(command, stoppingToken);
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                LogCleanupFailed(exception);
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to delete expired files")]
    private partial void LogCleanupFailed(Exception exception);
}
