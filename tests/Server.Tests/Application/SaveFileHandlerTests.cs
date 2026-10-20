using System.Security.Cryptography;
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
        _repository.AddAsync(Arg.Any<SharedFile>(), Arg.Any<CancellationToken>()).Returns(true);
        _handler = new SaveFileHandler(_repository, _storage, _passwordHasher);
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
        var id = (await _handler.HandleAsync(command, CancellationToken.None)).Id;

        // Assert
        await _storage.Received(1).SaveAsync(id, command.Content, CancellationToken.None);
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
        await _repository.Received(1).AddAsync(Arg.Is<SharedFile>(f => f.ExpiresAt == expiresAt), CancellationToken.None);
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
        await _repository.Received(1).AddAsync(Arg.Is<SharedFile>(f => f.DownloadsLeft == downloadsLeft), CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_DownloadsLeftNotGiven_AddsFileWithoutLimit()
    {
        // Arrange
        var command = CreateCommand(new MemoryStream());

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Is<SharedFile>(f => f.DownloadsLeft == null), CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_ExpiresAtNotGiven_AddsFileWithoutExpiration()
    {
        // Arrange
        var command = CreateCommand(new MemoryStream());

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Is<SharedFile>(f => f.ExpiresAt == null), CancellationToken.None);
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
        await _repository.Received(1).AddAsync(Arg.Is<SharedFile>(f => f.PasswordHash == "hash"), CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_PasswordNotGiven_AddsFileWithoutPasswordHash()
    {
        // Arrange
        var command = CreateCommand(new MemoryStream());

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Is<SharedFile>(f => f.PasswordHash == null), CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_AddsFileWithSha256OfContent()
    {
        // Arrange
        var expected = Sha256.Parse(SHA256.HashData("hello"u8));
        var command = CreateCommand(new MemoryStream("hello"u8.ToArray()));

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Is<SharedFile>(f => f.Sha256 == expected), CancellationToken.None);
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
    public async Task HandleAsync_ContentStartsMidStream_SavesContentFromTheSamePosition()
    {
        // Arrange
        var content = new MemoryStream("xxhello"u8.ToArray()) { Position = 2 };
        long? positionOnSave = null;
        _storage.SaveAsync(Arg.Any<ShareId>(), Arg.Do<Stream>(stream => positionOnSave = stream.Position), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(CreateCommand(content), CancellationToken.None);

        // Assert
        Assert.Equal(2, positionOnSave);
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
    public async Task HandleAsync_IdAlreadyTaken_AddsFileWithAnotherId()
    {
        // Arrange
        var ids = new List<ShareId>();
        _repository.AddAsync(Arg.Do<SharedFile>(f => ids.Add(f.Id)), Arg.Any<CancellationToken>())
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
        _repository.AddAsync(Arg.Any<SharedFile>(), Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var act = () => _handler.HandleAsync(CreateCommand(new MemoryStream()), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);
        await _storage.DidNotReceiveWithAnyArgs().SaveAsync(default!, default!, default);
    }

    [Fact]
    public async Task HandleAsync_SaveFails_DeletesAddedFile()
    {
        // Arrange
        SharedFile? added = null;
        _repository.AddAsync(Arg.Do<SharedFile>(f => added = f), Arg.Any<CancellationToken>()).Returns(true);
        _storage.SaveAsync(Arg.Any<ShareId>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .ThrowsAsync<IOException>();

        // Act
        var act = () => _handler.HandleAsync(CreateCommand(new MemoryStream()), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<IOException>(act);
        await _repository.Received(1).DeleteAsync(added!.Id, Arg.Any<CancellationToken>());
    }

    private static SaveFileCommand CreateCommand(
        Stream content,
        DateTimeOffset? expiresAt = null,
        Password? password = null,
        DownloadsLeft? downloadsLeft = null) =>
        new(FileName.Parse("report.pdf"), ContentType.Parse("application/pdf"), content, expiresAt, password, downloadsLeft);
}
