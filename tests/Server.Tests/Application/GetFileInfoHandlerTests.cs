using System.Security.Cryptography;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Server.Application.GetFileInfo;
using Server.Domain;
using Server.Domain.Abstractions;
using Server.Domain.ValueObjects;

namespace Server.Tests.Application;

public class GetFileInfoHandlerTests
{
    private readonly ISharedFileRepository _repository = Substitute.For<ISharedFileRepository>();
    private readonly IFileStorage _storage = Substitute.For<IFileStorage>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly GetFileInfoHandler _handler;

    public GetFileInfoHandlerTests() => _handler = new GetFileInfoHandler(_repository, _storage, _passwordHasher, NullLogger<GetFileInfoHandler>.Instance);

    [Fact]
    public async Task HandleAsync_FileExists_ReturnsFileInfo()
    {
        // Arrange
        var id = ShareId.New();
        var file = new SharedFile(id, FileName.Parse("report.pdf"), ContentType.Parse("application/pdf"), FileSize.Parse(5), Sha256.Parse(SHA256.HashData("hello"u8)));
        _repository.GetAsync(id, CancellationToken.None).Returns(file);
        _storage.ExistsAsync(id, CancellationToken.None).Returns(true);

        // Act
        var result = await _handler.HandleAsync(new GetFileInfoQuery(id), CancellationToken.None);

        // Assert
        var found = Assert.IsType<GetFileInfoResult.Found>(result);
        Assert.Equal(file.FileName, found.FileName);
        Assert.Equal(file.ContentType, found.ContentType);
        Assert.Equal(file.Size, found.Size);
    }

    [Fact]
    public async Task HandleAsync_FileExists_ReturnsSha256()
    {
        // Arrange
        var id = ShareId.New();
        var sha256 = Sha256.Parse(SHA256.HashData("world"u8));
        var file = new SharedFile(id, FileName.Parse("report.pdf"), ContentType.Parse("application/pdf"), FileSize.Parse(5), sha256);
        _repository.GetAsync(id, CancellationToken.None).Returns(file);
        _storage.ExistsAsync(id, CancellationToken.None).Returns(true);

        // Act
        var result = await _handler.HandleAsync(new GetFileInfoQuery(id), CancellationToken.None);

        // Assert
        var found = Assert.IsType<GetFileInfoResult.Found>(result);
        Assert.Equal(sha256, found.Sha256);
    }

    [Fact]
    public async Task HandleAsync_FileNotInRepository_ReturnsNotFound()
    {
        // Arrange
        var id = ShareId.New();
        _repository.GetAsync(id, CancellationToken.None).Returns((SharedFile?)null);

        // Act
        var result = await _handler.HandleAsync(new GetFileInfoQuery(id), CancellationToken.None);

        // Assert
        Assert.IsType<GetFileInfoResult.NotFound>(result);
    }

    [Fact]
    public async Task HandleAsync_FileNotInStorage_ReturnsNotFound()
    {
        // Arrange
        var id = ShareId.New();
        var file = new SharedFile(id, FileName.Parse("report.pdf"), ContentType.Parse("application/pdf"), FileSize.Parse(5), Sha256.Parse(SHA256.HashData("hello"u8)));
        _repository.GetAsync(id, CancellationToken.None).Returns(file);
        _storage.ExistsAsync(id, CancellationToken.None).Returns(false);

        // Act
        var result = await _handler.HandleAsync(new GetFileInfoQuery(id), CancellationToken.None);

        // Assert
        Assert.IsType<GetFileInfoResult.NotFound>(result);
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
            Sha256.Parse(SHA256.HashData("hello"u8)),
            DateTimeOffset.UtcNow.AddMinutes(-1));
        _repository.GetAsync(id, CancellationToken.None).Returns(file);
        _storage.ExistsAsync(id, CancellationToken.None).Returns(true);

        // Act
        var result = await _handler.HandleAsync(new GetFileInfoQuery(id), CancellationToken.None);

        // Assert
        Assert.IsType<GetFileInfoResult.NotFound>(result);
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
            Sha256.Parse(SHA256.HashData("hello"u8)),
            expiresAt);
        _repository.GetAsync(id, CancellationToken.None).Returns(file);
        _storage.ExistsAsync(id, CancellationToken.None).Returns(true);

        // Act
        var result = await _handler.HandleAsync(new GetFileInfoQuery(id), CancellationToken.None);

        // Assert
        var found = Assert.IsType<GetFileInfoResult.Found>(result);
        Assert.Equal(expiresAt, found.ExpiresAt);
    }

    [Fact]
    public async Task HandleAsync_ProtectedFileWithMatchingPassword_ReturnsFound()
    {
        // Arrange
        var id = ShareId.New();
        _repository.GetAsync(id, CancellationToken.None).Returns(CreateProtectedFile(id, "hash"));
        _storage.ExistsAsync(id, CancellationToken.None).Returns(true);
        var password = Password.Parse("secret");
        _passwordHasher.Verify(password, "hash").Returns(true);

        // Act
        var result = await _handler.HandleAsync(new GetFileInfoQuery(id, password), CancellationToken.None);

        // Assert
        Assert.IsType<GetFileInfoResult.Found>(result);
    }

    [Fact]
    public async Task HandleAsync_ProtectedFileWithWrongPassword_ReturnsAccessDenied()
    {
        // Arrange
        var id = ShareId.New();
        _repository.GetAsync(id, CancellationToken.None).Returns(CreateProtectedFile(id, "hash"));
        _storage.ExistsAsync(id, CancellationToken.None).Returns(true);
        var password = Password.Parse("wrong");
        _passwordHasher.Verify(password, "hash").Returns(false);

        // Act
        var result = await _handler.HandleAsync(new GetFileInfoQuery(id, password), CancellationToken.None);

        // Assert
        Assert.IsType<GetFileInfoResult.AccessDenied>(result);
    }

    [Fact]
    public async Task HandleAsync_ProtectedFileWithoutPassword_ReturnsAccessDenied()
    {
        // Arrange
        var id = ShareId.New();
        _repository.GetAsync(id, CancellationToken.None).Returns(CreateProtectedFile(id, "hash"));
        _storage.ExistsAsync(id, CancellationToken.None).Returns(true);

        // Act
        var result = await _handler.HandleAsync(new GetFileInfoQuery(id), CancellationToken.None);

        // Assert
        Assert.IsType<GetFileInfoResult.AccessDenied>(result);
    }

    [Fact]
    public async Task HandleAsync_FileExhausted_ReturnsNotFound()
    {
        // Arrange
        var id = ShareId.New();
        _repository.GetAsync(id, CancellationToken.None).Returns(CreateLimitedFile(id, 0));
        _storage.ExistsAsync(id, CancellationToken.None).Returns(true);

        // Act
        var result = await _handler.HandleAsync(new GetFileInfoQuery(id), CancellationToken.None);

        // Assert
        Assert.IsType<GetFileInfoResult.NotFound>(result);
    }

    [Fact]
    public async Task HandleAsync_LimitedFile_ReturnsDownloadsLeftWithoutSpending()
    {
        // Arrange
        var id = ShareId.New();
        _repository.GetAsync(id, CancellationToken.None).Returns(CreateLimitedFile(id, 3));
        _storage.ExistsAsync(id, CancellationToken.None).Returns(true);

        // Act
        var result = await _handler.HandleAsync(new GetFileInfoQuery(id), CancellationToken.None);

        // Assert
        var found = Assert.IsType<GetFileInfoResult.Found>(result);
        Assert.Equal(DownloadsLeft.Parse(3), found.DownloadsLeft);
        await _repository.DidNotReceive().SpendDownloadAsync(Arg.Any<ShareId>(), Arg.Any<CancellationToken>());
    }

    private static SharedFile CreateLimitedFile(ShareId id, int downloadsLeft) => new(
        id,
        FileName.Parse("report.pdf"),
        ContentType.Parse("application/pdf"),
        FileSize.Parse(5),
        Sha256.Parse(SHA256.HashData("hello"u8)),
        downloadsLeft: DownloadsLeft.Parse(downloadsLeft));

    private static SharedFile CreateProtectedFile(ShareId id, string passwordHash) => new(
        id,
        FileName.Parse("report.pdf"),
        ContentType.Parse("application/pdf"),
        FileSize.Parse(5),
        Sha256.Parse(SHA256.HashData("hello"u8)),
        passwordHash: passwordHash);
}
