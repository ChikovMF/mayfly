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
    private readonly GetFileHandler _handler;

    public GetFileHandlerTests() => _handler = new GetFileHandler(_repository, _storage);

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
        Assert.NotNull(result);
        Assert.Equal(file.FileName, result.FileName);
        Assert.Equal(file.ContentType, result.ContentType);
        Assert.Equal(file.Size, result.Size);
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
        Assert.NotNull(result);
        await result.CopyContentToAsync(destination, CancellationToken.None);
        await _storage.Received(1).CopyToAsync(id, destination, CancellationToken.None);
    }

    [Fact]
    public async Task HandleAsync_FileNotInRepository_ReturnsNull()
    {
        // Arrange
        var id = ShareId.New();
        _repository.GetAsync(id, CancellationToken.None).Returns((SharedFile?)null);

        // Act
        var result = await _handler.HandleAsync(new GetFileQuery(id), CancellationToken.None);

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
        var result = await _handler.HandleAsync(new GetFileQuery(id), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}
