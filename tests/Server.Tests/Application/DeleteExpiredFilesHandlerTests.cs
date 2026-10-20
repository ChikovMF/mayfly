using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Server.Application.DeleteExpiredFiles;
using Server.Domain.Abstractions;
using Server.Domain.ValueObjects;

namespace Server.Tests.Application;

public class DeleteExpiredFilesHandlerTests
{
    private readonly ISharedFileRepository _repository = Substitute.For<ISharedFileRepository>();
    private readonly IFileStorage _storage = Substitute.For<IFileStorage>();
    private readonly DeleteExpiredFilesHandler _handler;

    public DeleteExpiredFilesHandlerTests() => _handler = new DeleteExpiredFilesHandler(
        _repository,
        _storage,
        NullLogger<DeleteExpiredFilesHandler>.Instance);

    [Fact]
    public async Task HandleAsync_ExpiredFileClaimed_DeletesContentBeforeRecord()
    {
        // Arrange
        var id = ShareId.New();
        IReadOnlyList<ExpiredFileClaim> claimed = [new ExpiredFileClaim(id, 0, FileDeletionReason.Expired)];
        IReadOnlyList<ExpiredFileClaim> noMoreClaimed = [];
        _repository.ClaimExpiredAsync(default, default, default, default, default).ReturnsForAnyArgs(claimed, noMoreClaimed);

        // Act
        await _handler.HandleAsync(CreateCommand(), CancellationToken.None);

        // Assert
        Received.InOrder(() =>
        {
            _storage.DeleteAsync(id, CancellationToken.None);
            _repository.DeleteAsync(id, CancellationToken.None);
        });
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_ClaimsWithCommandLeaseExhaustedDelayAndMaxAttempts()
    {
        // Arrange
        var command = new DeleteExpiredFilesCommand(TimeSpan.FromMinutes(7), TimeSpan.FromMinutes(30), 3);
        IReadOnlyList<ExpiredFileClaim> noneClaimed = [];
        _repository.ClaimExpiredAsync(default, default, default, default, default).ReturnsForAnyArgs(noneClaimed);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).ClaimExpiredAsync(
            Arg.Any<int>(),
            command.Lease,
            command.ExhaustedFileDelay,
            command.MaxAttempts,
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_NoExpiredFiles_DeletesNothing()
    {
        // Arrange
        IReadOnlyList<ExpiredFileClaim> noneClaimed = [];
        _repository.ClaimExpiredAsync(default, default, default, default, default).ReturnsForAnyArgs(noneClaimed);

        // Act
        await _handler.HandleAsync(CreateCommand(), CancellationToken.None);

        // Assert
        await _storage.DidNotReceiveWithAnyArgs().DeleteAsync(default!, default);
        await _repository.DidNotReceiveWithAnyArgs().DeleteAsync(default!, default);
    }

    [Fact]
    public async Task HandleAsync_DeletionFails_PostponesFileAndKeepsRecord()
    {
        // Arrange
        var id = ShareId.New();
        IReadOnlyList<ExpiredFileClaim> claimed = [new ExpiredFileClaim(id, 0, FileDeletionReason.Expired)];
        IReadOnlyList<ExpiredFileClaim> noMoreClaimed = [];
        _repository.ClaimExpiredAsync(default, default, default, default, default).ReturnsForAnyArgs(claimed, noMoreClaimed);
        _storage.DeleteAsync(id, CancellationToken.None).Returns(Task.FromException(new IOException()));

        // Act
        await _handler.HandleAsync(CreateCommand(), CancellationToken.None);

        // Assert
        await _repository.Received(1).PostponeDeletionAsync(id, Arg.Any<TimeSpan>(), CancellationToken.None);
        await _repository.DidNotReceiveWithAnyArgs().DeleteAsync(default!, default);
    }

    [Fact]
    public async Task HandleAsync_DeletionFails_DeletesNextFile()
    {
        // Arrange
        var failing = ShareId.New();
        var next = ShareId.New();
        IReadOnlyList<ExpiredFileClaim> claimed = [new ExpiredFileClaim(failing, 0, FileDeletionReason.Expired), new ExpiredFileClaim(next, 0, FileDeletionReason.Expired)];
        IReadOnlyList<ExpiredFileClaim> noMoreClaimed = [];
        _repository.ClaimExpiredAsync(default, default, default, default, default).ReturnsForAnyArgs(claimed, noMoreClaimed);
        _storage.DeleteAsync(failing, CancellationToken.None).Returns(Task.FromException(new IOException()));

        // Act
        await _handler.HandleAsync(CreateCommand(), CancellationToken.None);

        // Assert
        await _repository.Received(1).DeleteAsync(next, CancellationToken.None);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(3, 8)]
    [InlineData(30, 24 * 60)]
    public async Task HandleAsync_DeletionFails_PostponesByGrowingDelay(int failedAttempts, int expectedMinutes)
    {
        // Arrange
        var id = ShareId.New();
        IReadOnlyList<ExpiredFileClaim> claimed = [new ExpiredFileClaim(id, failedAttempts, FileDeletionReason.Expired)];
        IReadOnlyList<ExpiredFileClaim> noMoreClaimed = [];
        _repository.ClaimExpiredAsync(default, default, default, default, default).ReturnsForAnyArgs(claimed, noMoreClaimed);
        _storage.DeleteAsync(id, CancellationToken.None).Returns(Task.FromException(new IOException()));

        // Act
        await _handler.HandleAsync(CreateCommand(), CancellationToken.None);

        // Assert
        await _repository.Received(1).PostponeDeletionAsync(
            id,
            TimeSpan.FromMinutes(expectedMinutes),
            CancellationToken.None);
    }

    private static DeleteExpiredFilesCommand CreateCommand() => new(TimeSpan.FromMinutes(5), TimeSpan.FromHours(1), 10);
}
