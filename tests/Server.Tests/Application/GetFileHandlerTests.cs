using NSubstitute;
using Server.Application.GetFile;
using Server.Domain;
using Server.Domain.Abstractions;
using Server.Domain.ValueObjects;

namespace Server.Tests.Application;

public class GetFileHandlerTests
{
    private readonly ISharedFileRepository _repository = Substitute.For<ISharedFileRepository>();
    private readonly IFileStorage _storage = Substitute.For<IFileStorage>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly GetFileHandler _handler;

    public GetFileHandlerTests() => _handler = new GetFileHandler(_repository, _storage, _passwordHasher);

    [Fact]
    public async Task HandleAsync_FileExists_ReturnsFileData()
    {
        // Arrange
        var id = ShareId.New();
        var file = new SharedFile(id, FileName.Parse("report.pdf"), ContentType.Parse("application/pdf"), FileSize.Parse(5));
        _repository.GetAsync(id, CancellationToken.None).Returns(file);
        _storage.ExistsAsync(id, CancellationToken.None).Returns(true);

        // Act
        var result = await _handler.HandleAsync(new GetFileQuery(id), CancellationToken.None);

        // Assert
        var found = Assert.IsType<GetFileResult.Found>(result);
        Assert.Equal(file.FileName, found.FileName);
        Assert.Equal(file.ContentType, found.ContentType);
        Assert.Equal(file.Size, found.Size);
    }

    [Fact]
    public async Task HandleAsync_FileExists_ReturnsContentCopiedFromStorage()
    {
        // Arrange
        var id = ShareId.New();
        var file = new SharedFile(id, FileName.Parse("report.pdf"), ContentType.Parse("application/pdf"), FileSize.Parse(5));
        _repository.GetAsync(id, CancellationToken.None).Returns(file);
        _storage.ExistsAsync(id, CancellationToken.None).Returns(true);
        using var destination = new MemoryStream();

        // Act
        var result = await _handler.HandleAsync(new GetFileQuery(id), CancellationToken.None);

        // Assert
        var found = Assert.IsType<GetFileResult.Found>(result);
        await found.CopyContentToAsync(destination, CancellationToken.None);
        await _storage.Received(1).CopyToAsync(id, destination, CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_FileNotInRepository_ReturnsNotFound()
    {
        // Arrange
        var id = ShareId.New();
        _repository.GetAsync(id, CancellationToken.None).Returns((SharedFile?)null);

        // Act
        var result = await _handler.HandleAsync(new GetFileQuery(id), CancellationToken.None);

        // Assert
        Assert.IsType<GetFileResult.NotFound>(result);
    }

    [Fact]
    public async Task HandleAsync_FileNotInStorage_ReturnsNotFound()
    {
        // Arrange
        var id = ShareId.New();
        var file = new SharedFile(id, FileName.Parse("report.pdf"), ContentType.Parse("application/pdf"), FileSize.Parse(5));
        _repository.GetAsync(id, CancellationToken.None).Returns(file);
        _storage.ExistsAsync(id, CancellationToken.None).Returns(false);

        // Act
        var result = await _handler.HandleAsync(new GetFileQuery(id), CancellationToken.None);

        // Assert
        Assert.IsType<GetFileResult.NotFound>(result);
    }

    [Fact]
    public async Task HandleAsync_FileExpired_ReturnsNotFound()
    {
        // Arrange
        var id = ShareId.New();
        var file = new SharedFile(
            id,
            FileName.Parse("report.pdf"),
            ContentType.Parse("application/pdf"),
            FileSize.Parse(5),
            DateTimeOffset.UtcNow.AddMinutes(-1));
        _repository.GetAsync(id, CancellationToken.None).Returns(file);
        _storage.ExistsAsync(id, CancellationToken.None).Returns(true);

        // Act
        var result = await _handler.HandleAsync(new GetFileQuery(id), CancellationToken.None);

        // Assert
        Assert.IsType<GetFileResult.NotFound>(result);
    }

    [Fact]
    public async Task HandleAsync_FileWithExpiration_ReturnsExpiresAt()
    {
        // Arrange
        var id = ShareId.New();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var file = new SharedFile(
            id,
            FileName.Parse("report.pdf"),
            ContentType.Parse("application/pdf"),
            FileSize.Parse(5),
            expiresAt);
        _repository.GetAsync(id, CancellationToken.None).Returns(file);
        _storage.ExistsAsync(id, CancellationToken.None).Returns(true);

        // Act
        var result = await _handler.HandleAsync(new GetFileQuery(id), CancellationToken.None);

        // Assert
        var found = Assert.IsType<GetFileResult.Found>(result);
        Assert.Equal(expiresAt, found.ExpiresAt);
    }

    [Fact]
    public async Task HandleAsync_ProtectedFileWithMatchingPassword_ReturnsFound()
    {
        // Arrange
        var id = ShareId.New();
        var password = Password.Parse("secret");
        _repository.GetAsync(id, CancellationToken.None).Returns(CreateProtectedFile(id, "hash"));
        _storage.ExistsAsync(id, CancellationToken.None).Returns(true);
        _passwordHasher.Verify(password, "hash").Returns(true);

        // Act
        var result = await _handler.HandleAsync(new GetFileQuery(id, password), CancellationToken.None);

        // Assert
        Assert.IsType<GetFileResult.Found>(result);
    }

    [Fact]
    public async Task HandleAsync_ProtectedFileWithWrongPassword_ReturnsAccessDenied()
    {
        // Arrange
        var id = ShareId.New();
        var password = Password.Parse("wrong");
        _repository.GetAsync(id, CancellationToken.None).Returns(CreateProtectedFile(id, "hash"));
        _storage.ExistsAsync(id, CancellationToken.None).Returns(true);
        _passwordHasher.Verify(password, "hash").Returns(false);

        // Act
        var result = await _handler.HandleAsync(new GetFileQuery(id, password), CancellationToken.None);

        // Assert
        Assert.IsType<GetFileResult.AccessDenied>(result);
    }

    [Fact]
    public async Task HandleAsync_ProtectedFileWithoutPassword_ReturnsAccessDenied()
    {
        // Arrange
        var id = ShareId.New();
        _repository.GetAsync(id, CancellationToken.None).Returns(CreateProtectedFile(id, "hash"));
        _storage.ExistsAsync(id, CancellationToken.None).Returns(true);

        // Act
        var result = await _handler.HandleAsync(new GetFileQuery(id), CancellationToken.None);

        // Assert
        Assert.IsType<GetFileResult.AccessDenied>(result);
    }

    private static SharedFile CreateProtectedFile(ShareId id, string passwordHash) => new(
        id,
        FileName.Parse("report.pdf"),
        ContentType.Parse("application/pdf"),
        FileSize.Parse(5),
        passwordHash: passwordHash);
}
