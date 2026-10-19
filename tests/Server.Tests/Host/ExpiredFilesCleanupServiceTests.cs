using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Server.Application.DeleteExpiredFiles;
using Server.Host.Cleanup;

namespace Server.Tests.Host;

public class ExpiredFilesCleanupServiceTests
{
    [Fact]
    public async Task StartAsync_CleanupTimesOut_RunsNextCleanup()
    {
        // Arrange
        var secondRun = new TaskCompletionSource();
        var handler = Substitute.For<IDeleteExpiredFilesHandler>();
        handler.HandleAsync(Arg.Any<DeleteExpiredFilesCommand>(), Arg.Any<CancellationToken>()).Returns(
            _ => Task.FromException(new TaskCanceledException()),
            _ =>
            {
                secondRun.TrySetResult();
                return Task.CompletedTask;
            });

        await using var services = new ServiceCollection().AddScoped(_ => handler).BuildServiceProvider();
        var service = new ExpiredFilesCleanupService(
            services.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new CleanupOptions { Interval = TimeSpan.FromMilliseconds(10) }),
            NullLogger<ExpiredFilesCleanupService>.Instance);

        // Act
        await service.StartAsync(CancellationToken.None);
        var act = () => secondRun.Task.WaitAsync(TimeSpan.FromSeconds(10));

        // Assert
        await act();
        await service.StopAsync(CancellationToken.None);
    }
}
