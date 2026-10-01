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
    private readonly ShareId _id = ShareId.New();

    public GetFileHandlerTests() => _handler = new GetFileHandler(_repository, _storage);

    [Fact]
    public async Task HandleAsync_FileExists_ReturnsFileDataAndContent()
    {
        // Arrange
        var file = new SharedFile(_id, FileName.Parse("report.pdf"), ContentType.Parse("application/pdf"), FileSize.Parse(5));
        using var content = new MemoryStream();
        _repository.GetAsync(_id, CancellationToken.None).Returns(file);
        _storage.OpenReadAsync(_id, CancellationToken.None).Returns(content);

        // Act
        var result = await _handler.HandleAsync(new GetFileQuery(_id), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(file.FileName, result.FileName);
        Assert.Equal(file.ContentType, result.ContentType);
        Assert.Same(content, result.Content);
    }

    [Fact]
    public async Task HandleAsync_FileNotInRepository_ReturnsNullWithoutOpeningContent()
    {
        // Arrange
        _repository.GetAsync(_id, CancellationToken.None).Returns((SharedFile?)null);

        // Act
        var result = await _handler.HandleAsync(new GetFileQuery(_id), CancellationToken.None);

        // Assert
        Assert.Null(result);
        await _storage.DidNotReceiveWithAnyArgs().OpenReadAsync(default!, default);
    }

    [Fact]
    public async Task HandleAsync_ContentNotInStorage_ReturnsNull()
    {
        // Arrange
        var file = new SharedFile(_id, FileName.Parse("report.pdf"), ContentType.Parse("application/pdf"), FileSize.Parse(5));
        _repository.GetAsync(_id, CancellationToken.None).Returns(file);
        _storage.OpenReadAsync(_id, CancellationToken.None).Returns((Stream?)null);

        // Act
        var result = await _handler.HandleAsync(new GetFileQuery(_id), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}
