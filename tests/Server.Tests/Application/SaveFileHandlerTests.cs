using System.Security.Cryptography;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Server.Application.SaveFile;
using Server.Domain;
using Server.Domain.Abstractions;
using Server.Domain.ValueObjects;

namespace Server.Tests.Application;

public class SaveFileHandlerTests
{
    private readonly ISharedFileRepository _repository = Substitute.For<ISharedFileRepository>();
    private readonly IFileStorage _storage = Substitute.For<IFileStorage>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly SaveFileHandler _handler;

    public SaveFileHandlerTests()
    {
        _repository.AddAsync(Arg.Any<PendingFile>(), Arg.Any<CancellationToken>()).Returns(true);
        _repository.CompleteAsync(Arg.Any<ShareId>(), Arg.Any<Sha256>(), Arg.Any<CancellationToken>()).Returns(true);
        _storage.SaveAsync(default!, default!, default!, default)
            .ReturnsForAnyArgs(call => call.ArgAt<Stream>(1).CopyToAsync(Stream.Null));
        _handler = new SaveFileHandler(_repository, _storage, _passwordHasher, NullLogger<SaveFileHandler>.Instance);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_AddsFileWithCommandData()
    {
        // Arrange
        var command = CreateCommand(new MemoryStream(new byte[5]));

        // Act
        var id = (await _handler.HandleAsync(command, CancellationToken.None)).Id;

        // Assert
        await _repository.Received(1).AddAsync(
            Arg.Is<PendingFile>(f =>
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
        var id = (await _handler.HandleAsync(command, CancellationToken.None)).Id;

        // Assert
        await _storage.Received(1).SaveAsync(id, Arg.Any<Stream>(), command.Size, CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_ExpiresAtGiven_AddsFileWithGivenExpiration()
    {
        // Arrange
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var command = CreateCommand(new MemoryStream(), expiresAt);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Is<PendingFile>(f => f.ExpiresAt == expiresAt), CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_DownloadsLeftGiven_AddsFileWithGivenDownloadsLeft()
    {
        // Arrange
        var downloadsLeft = DownloadsLeft.Parse(3);
        var command = CreateCommand(new MemoryStream(), downloadsLeft: downloadsLeft);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Is<PendingFile>(f => f.DownloadsLeft == downloadsLeft), CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_DownloadsLeftNotGiven_AddsFileWithoutLimit()
    {
        // Arrange
        var command = CreateCommand(new MemoryStream());

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Is<PendingFile>(f => f.DownloadsLeft == null), CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_ExpiresAtNotGiven_AddsFileWithoutExpiration()
    {
        // Arrange
        var command = CreateCommand(new MemoryStream());

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Is<PendingFile>(f => f.ExpiresAt == null), CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_PasswordGiven_AddsFileWithPasswordHash()
    {
        // Arrange
        var password = Password.Parse("secret");
        _passwordHasher.Hash(password).Returns("hash");
        var command = CreateCommand(new MemoryStream(), password: password);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Is<PendingFile>(f => f.PasswordHash == "hash"), CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_PasswordNotGiven_AddsFileWithoutPasswordHash()
    {
        // Arrange
        var command = CreateCommand(new MemoryStream());

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Is<PendingFile>(f => f.PasswordHash == null), CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CompletesFileWithSha256OfContent()
    {
        // Arrange
        var expected = Sha256.Parse(SHA256.HashData("hello"u8));
        var command = CreateCommand(new MemoryStream("hello"u8.ToArray()));

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).CompleteAsync(Arg.Any<ShareId>(), expected, CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsSha256OfContent()
    {
        // Arrange
        var expected = Sha256.Parse(SHA256.HashData("hello"u8));
        var command = CreateCommand(new MemoryStream("hello"u8.ToArray()));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(expected, result.Sha256);
    }

    [Fact]
    public async Task HandleAsync_ContentStartsMidStream_HashesOnlyTheRest()
    {
        // Arrange
        var content = new MemoryStream("xxhello"u8.ToArray()) { Position = 2 };

        // Act
        var result = await _handler.HandleAsync(CreateCommand(content), CancellationToken.None);

        // Assert
        Assert.Equal(Sha256.Parse(SHA256.HashData("hello"u8)), result.Sha256);
    }

    [Fact]
    public async Task HandleAsync_ContentStartsMidStream_SavesOnlyTheRest()
    {
        // Arrange
        var content = new MemoryStream("xxhello"u8.ToArray()) { Position = 2 };
        var saved = new MemoryStream();
        _storage.SaveAsync(Arg.Any<ShareId>(), Arg.Do<Stream>(stream => stream.CopyTo(saved)), Arg.Any<FileSize>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(CreateCommand(content), CancellationToken.None);

        // Assert
        Assert.Equal("hello"u8.ToArray(), saved.ToArray());
    }

    [Fact]
    public async Task HandleAsync_ContentDiffersFromDeclaredSize_ThrowsArgumentExceptionAndDeletesContent()
    {
        // Arrange
        var command = CreateCommand(new MemoryStream("hello"u8.ToArray()), size: 3);

        // Act
        var act = () => _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(act);
        await _repository.DidNotReceiveWithAnyArgs().CompleteAsync(default!, default!, default);
        await _storage.ReceivedWithAnyArgs(1).DeleteAsync(default!, default);
    }

    [Fact]
    public async Task HandleAsync_AddFails_DoesNotSaveContent()
    {
        // Arrange
        _repository.AddAsync(Arg.Any<PendingFile>(), Arg.Any<CancellationToken>()).ThrowsAsync<IOException>();

        // Act
        var act = () => _handler.HandleAsync(CreateCommand(new MemoryStream()), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<IOException>(act);
        await _storage.DidNotReceiveWithAnyArgs().SaveAsync(default!, default!, default!, default);
    }

    [Fact]
    public async Task HandleAsync_IdAlreadyTaken_AddsFileWithAnotherId()
    {
        // Arrange
        var ids = new List<ShareId>();
        _repository.AddAsync(Arg.Do<PendingFile>(f => ids.Add(f.Id)), Arg.Any<CancellationToken>())
            .Returns(false, true);

        // Act
        var id = (await _handler.HandleAsync(CreateCommand(new MemoryStream()), CancellationToken.None)).Id;

        // Assert
        Assert.Equal(2, ids.Count);
        Assert.NotEqual(ids[0], ids[1]);
        Assert.Equal(ids[1], id);
    }

    [Fact]
    public async Task HandleAsync_NoFreeId_ThrowsInvalidOperationException()
    {
        // Arrange
        _repository.AddAsync(Arg.Any<PendingFile>(), Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var act = () => _handler.HandleAsync(CreateCommand(new MemoryStream()), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
        await _storage.DidNotReceiveWithAnyArgs().SaveAsync(default!, default!, default!, default);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CompletesUploadAfterSavingContent()
    {
        // Arrange
        var command = CreateCommand(new MemoryStream());

        // Act
        var id = (await _handler.HandleAsync(command, CancellationToken.None)).Id;

        // Assert
        Received.InOrder(() =>
        {
            _storage.SaveAsync(id, Arg.Any<Stream>(), command.Size, CancellationToken.None);
            _repository.CompleteAsync(id, Arg.Any<Sha256>(), CancellationToken.None);
        });
    }

    [Fact]
    public async Task HandleAsync_SaveFails_DeletesContentBeforeRecord()
    {
        // Arrange
        PendingFile? added = null;
        _repository.AddAsync(Arg.Do<PendingFile>(f => added = f), Arg.Any<CancellationToken>()).Returns(true);
        _storage.SaveAsync(Arg.Any<ShareId>(), Arg.Any<Stream>(), Arg.Any<FileSize>(), Arg.Any<CancellationToken>())
            .ThrowsAsync<IOException>();

        // Act
        var act = () => _handler.HandleAsync(CreateCommand(new MemoryStream()), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<IOException>(act);
        Received.InOrder(() =>
        {
            _storage.DeleteAsync(added!.Id, Arg.Any<CancellationToken>());
            _repository.DeleteAsync(added!.Id, Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task HandleAsync_UploadAbandonedMeanwhile_ThrowsUploadAbandonedExceptionAndDeletesContent()
    {
        // Arrange
        _repository.CompleteAsync(Arg.Any<ShareId>(), Arg.Any<Sha256>(), Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var act = () => _handler.HandleAsync(CreateCommand(new MemoryStream()), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<UploadAbandonedException>(act);
        await _storage.ReceivedWithAnyArgs(1).DeleteAsync(default!, default);
    }

    private static SaveFileCommand CreateCommand(
        Stream content,
        DateTimeOffset? expiresAt = null,
        Password? password = null,
        DownloadsLeft? downloadsLeft = null,
        long? size = null) =>
        new(
            FileName.Parse("report.pdf"),
            ContentType.Parse("application/pdf"),
            content,
            FileSize.Parse(size ?? content.Length - content.Position),
            expiresAt,
            password,
            downloadsLeft);
}
