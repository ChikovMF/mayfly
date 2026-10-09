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
    public async Task HandleAsync_ExpiredFilesExist_DeletesContentBeforeRecord()
    {
        // Arrange
        var id = ShareId.New();
        _repository.GetExpiredIdsAsync(Arg.Any<int>(), CancellationToken.None).Returns([id], []);

        // Act
        await _handler.HandleAsync(CancellationToken.None);

        // Assert
        Received.InOrder(() =>
        {
            _storage.DeleteAsync(id, CancellationToken.None);
            _repository.DeleteAsync(id, CancellationToken.None);
        });
    }

    [Fact]
    public async Task HandleAsync_NoExpiredFiles_DeletesNothing()
    {
        // Arrange
        _repository.GetExpiredIdsAsync(Arg.Any<int>(), CancellationToken.None).Returns([]);

        // Act
        await _handler.HandleAsync(CancellationToken.None);

        // Assert
        await _storage.DidNotReceiveWithAnyArgs().DeleteAsync(default!, default);
        await _repository.DidNotReceiveWithAnyArgs().DeleteAsync(default!, default);
    }

    [Fact]
    public async Task HandleAsync_StorageDeleteFails_KeepsRecord()
    {
        // Arrange
        var id = ShareId.New();
        _repository.GetExpiredIdsAsync(Arg.Any<int>(), CancellationToken.None).Returns([id]);
        _storage.DeleteAsync(id, CancellationToken.None).Returns(Task.FromException(new IOException()));

        // Act
        var act = () => _handler.HandleAsync(CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<IOException>(act);
        await _repository.DidNotReceiveWithAnyArgs().DeleteAsync(default!, default);
    }
}
