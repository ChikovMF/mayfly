using Mayfly.Client;
using Mayfly.Client.Models;

namespace Integration.Tests;

[Collection(MayflyCollection.Name)]
[Trait("Category", "Integration")]
public sealed class MayflyClientTests(MayflyFactory factory)
{
    private readonly IMayflyClient _client = factory.Client;

    [Fact]
    public async Task GetInfoAsync_UploadedFile_ReturnsFileInfo()
    {
        // Arrange
        var content = "Hello, Mayfly!"u8.ToArray();
        var uploaded = await _client.UploadAsync(new MemoryStream(content), "hello.txt", "text/plain");

        // Act
        var info = await _client.GetInfoAsync(uploaded.Id);

        // Assert
        Assert.Equal(new RemoteFileInfo("hello.txt", "text/plain", content.Length, null), info);
    }

    [Fact]
    public async Task DownloadAsync_UploadedFile_ReturnsFile()
    {
        // Arrange
        var content = "Hello, Mayfly!"u8.ToArray();
        var uploaded = await _client.UploadAsync(new MemoryStream(content), "hello.txt", "text/plain");

        // Act
        using var file = await _client.DownloadAsync(uploaded.Id);

        // Assert
        Assert.NotNull(file);
        Assert.Equal(new RemoteFileInfo("hello.txt", "text/plain", content.Length, null), file.Info);
        using var downloaded = new MemoryStream();
        await file.Content.CopyToAsync(downloaded);
        Assert.Equal(content, downloaded.ToArray());
    }

    [Fact]
    public async Task GetInfoAsync_FileUploadedWithExpiration_ReturnsExpiration()
    {
        // Arrange
        var expiresAt = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(2), TimeSpan.Zero);
        var uploaded = await _client.UploadAsync(new MemoryStream("Hello, Mayfly!"u8.ToArray()), "hello.txt", expiresAt: expiresAt);

        // Act
        var info = await _client.GetInfoAsync(uploaded.Id);

        // Assert
        Assert.NotNull(info);
        Assert.Equal(expiresAt, info.ExpiresAt);
    }
}
