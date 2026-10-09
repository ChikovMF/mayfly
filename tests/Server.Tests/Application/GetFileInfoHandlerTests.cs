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
    private readonly GetFileInfoHandler _handler;

    public GetFileInfoHandlerTests() => _handler = new GetFileInfoHandler(_repository, _storage);

    [Fact]
    public async Task HandleAsync_FileExists_ReturnsFileInfo()
    {
        // Arrange
        var id = ShareId.New();
        var file = new SharedFile(id, FileName.Parse("report.pdf"), ContentType.Parse("application/pdf"), FileSize.Parse(5));
        _repository.GetAsync(id, CancellationToken.None).Returns(file);
        _storage.ExistsAsync(id, CancellationToken.None).Returns(true);

        // Act
        var result = await _handler.HandleAsync(new GetFileInfoQuery(id), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(file.FileName, result.FileName);
        Assert.Equal(file.ContentType, result.ContentType);
        Assert.Equal(file.Size, result.Size);
    }

    [Fact]
    public async Task HandleAsync_FileNotInRepository_ReturnsNull()
    {
        // Arrange
        var id = ShareId.New();
        _repository.GetAsync(id, CancellationToken.None).Returns((SharedFile?)null);

        // Act
        var result = await _handler.HandleAsync(new GetFileInfoQuery(id), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task HandleAsync_FileNotInStorage_ReturnsNull()
    {
        // Arrange
        var id = ShareId.New();
        var file = new SharedFile(id, FileName.Parse("report.pdf"), ContentType.Parse("application/pdf"), FileSize.Parse(5));
        _repository.GetAsync(id, CancellationToken.None).Returns(file);
        _storage.ExistsAsync(id, CancellationToken.None).Returns(false);

        // Act
        var result = await _handler.HandleAsync(new GetFileInfoQuery(id), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task HandleAsync_FileExpired_ReturnsNull()
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
        var result = await _handler.HandleAsync(new GetFileInfoQuery(id), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}
