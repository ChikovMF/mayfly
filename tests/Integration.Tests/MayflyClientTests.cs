using System.Net;
using System.Security.Cryptography;
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
        Assert.Equal(new RemoteFileInfo("hello.txt", "text/plain", content.Length, null, null, Sha256Of(content)), info);
    }

    [Fact]
    public async Task UploadAsync_ReturnsSha256OfContent()
    {
        // Arrange
        var content = "Hello, Mayfly!"u8.ToArray();

        // Act
        var uploaded = await _client.UploadAsync(new MemoryStream(content), "hello.txt", "text/plain");

        // Assert
        Assert.Equal(Sha256Of(content), uploaded.Sha256);
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
        Assert.Equal(new RemoteFileInfo("hello.txt", "text/plain", content.Length, null, null, Sha256Of(content)), file.Info);
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

    [Fact]
    public async Task DownloadAsync_FileWithDownloadLimit_ReturnsDownloadsLeftAfterThisDownload()
    {
        // Arrange
        var uploaded = await _client.UploadAsync(new MemoryStream("Hello, Mayfly!"u8.ToArray()), "hello.txt", maxDownloads: 3);

        // Act
        using var file = await _client.DownloadAsync(uploaded.Id);

        // Assert
        Assert.NotNull(file);
        Assert.Equal(2, file.Info.DownloadsLeft);
    }

    [Fact]
    public async Task DownloadAsync_DownloadsExhausted_ReturnsNull()
    {
        // Arrange
        var uploaded = await _client.UploadAsync(new MemoryStream("Hello, Mayfly!"u8.ToArray()), "hello.txt", maxDownloads: 1);
        using (await _client.DownloadAsync(uploaded.Id))
        {
        }

        // Act
        var file = await _client.DownloadAsync(uploaded.Id);

        // Assert
        Assert.Null(file);
    }

    [Fact]
    public async Task GetInfoAsync_ProtectedFileWithItsPassword_ReturnsFileInfo()
    {
        // Arrange
        var content = "Hello, Mayfly!"u8.ToArray();
        var uploaded = await _client.UploadAsync(new MemoryStream(content), "hello.txt", "text/plain", password: "пароль 1");

        // Act
        var info = await _client.GetInfoAsync(uploaded.Id, "пароль 1");

        // Assert
        Assert.Equal(new RemoteFileInfo("hello.txt", "text/plain", content.Length, null, null, Sha256Of(content)), info);
    }

    [Fact]
    public async Task DownloadAsync_ProtectedFileWithItsPassword_ReturnsFile()
    {
        // Arrange
        var content = "Hello, Mayfly!"u8.ToArray();
        var uploaded = await _client.UploadAsync(new MemoryStream(content), "hello.txt", "text/plain", password: "пароль 1");

        // Act
        using var file = await _client.DownloadAsync(uploaded.Id, "пароль 1");

        // Assert
        Assert.NotNull(file);
        using var downloaded = new MemoryStream();
        await file.Content.CopyToAsync(downloaded);
        Assert.Equal(content, downloaded.ToArray());
    }

    [Fact]
    public async Task DownloadAsync_ProtectedFileWithoutPassword_ThrowsForbidden()
    {
        // Arrange
        var uploaded = await _client.UploadAsync(new MemoryStream("Hello, Mayfly!"u8.ToArray()), "hello.txt", password: "пароль 1");

        // Act
        var act = () => _client.DownloadAsync(uploaded.Id);

        // Assert
        var exception = await Assert.ThrowsAsync<MayflyApiException>(act);
        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }

    private static string Sha256Of(byte[] content) => Convert.ToHexStringLower(SHA256.HashData(content));
}
