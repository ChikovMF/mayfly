using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Server.Application;
using Server.Domain;
using Server.Domain.Abstractions;
using Server.Domain.ValueObjects;

namespace Server.Tests.Application;

public class SaveFileHandlerTests
{
    private readonly ISharedFileRepository _repository = Substitute.For<ISharedFileRepository>();
    private readonly IFileStorage _storage = Substitute.For<IFileStorage>();
    private readonly SaveFileHandler _handler;

    public SaveFileHandlerTests() => _handler = new SaveFileHandler(_repository, _storage);

    [Fact]
    public async Task HandleAsync_ValidCommand_AddsFileWithCommandData()
    {
        // Arrange
        var command = CreateCommand(new MemoryStream(new byte[5]));

        // Act
        var id = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(
            Arg.Is<SharedFile>(f =>
                f.Id == id &&
                f.FileName == command.FileName &&
                f.ContentType == command.ContentType &&
                f.Size.Bytes == 5),
            CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_SavesContentUnderReturnedId()
    {
        // Arrange
        var command = CreateCommand(new MemoryStream());

        // Act
        var id = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await _storage.Received(1).SaveAsync(id, command.Content, CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_NonSeekableContent_ThrowsArgumentException()
    {
        // Arrange
        var command = CreateCommand(Substitute.For<Stream>());

        // Act
        var act = () => _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task HandleAsync_AddFails_DoesNotSaveContent()
    {
        // Arrange
        _repository.AddAsync(Arg.Any<SharedFile>(), Arg.Any<CancellationToken>()).ThrowsAsync<IOException>();

        // Act
        var act = () => _handler.HandleAsync(CreateCommand(new MemoryStream()), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<IOException>(act);
        await _storage.DidNotReceiveWithAnyArgs().SaveAsync(default!, default!, default);
    }

    [Fact]
    public async Task HandleAsync_SaveFails_DeletesAddedFile()
    {
        // Arrange
        SharedFile? added = null;
        await _repository.AddAsync(Arg.Do<SharedFile>(f => added = f), Arg.Any<CancellationToken>());
        _storage.SaveAsync(Arg.Any<ShareId>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .ThrowsAsync<IOException>();

        // Act
        var act = () => _handler.HandleAsync(CreateCommand(new MemoryStream()), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<IOException>(act);
        await _repository.Received(1).DeleteAsync(added!.Id, Arg.Any<CancellationToken>());
    }

    private static SaveFileCommand CreateCommand(Stream content) =>
        new(FileName.Parse("report.pdf"), ContentType.Parse("application/pdf"), content);
}
