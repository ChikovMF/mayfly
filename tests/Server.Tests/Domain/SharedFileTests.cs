using Server.Domain;
using Server.Domain.ValueObjects;

namespace Server.Tests.Domain;

public class SharedFileTests
{
    [Fact]
    public void Constructor_ValidValues_ExposesGivenValues()
    {
        // Arrange
        var id = ShareId.New();
        var fileName = FileName.Parse("report.pdf");
        var contentType = ContentType.Parse("application/pdf");
        var size = FileSize.Parse(1024);

        // Act
        var file = new SharedFile(id, fileName, contentType, size);

        // Assert
        Assert.Same(id, file.Id);
        Assert.Same(fileName, file.FileName);
        Assert.Same(contentType, file.ContentType);
        Assert.Same(size, file.Size);
    }

    [Fact]
    public void IsExpired_NoExpiration_ReturnsFalse()
    {
        // Arrange
        var file = CreateFile(expiresAt: null);

        // Act
        var result = file.IsExpired();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsExpired_ExpirationInFuture_ReturnsFalse()
    {
        // Arrange
        var file = CreateFile(DateTimeOffset.UtcNow.AddMinutes(1));

        // Act
        var result = file.IsExpired();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsExpired_ExpirationInPast_ReturnsTrue()
    {
        // Arrange
        var file = CreateFile(DateTimeOffset.UtcNow.AddMinutes(-1));

        // Act
        var result = file.IsExpired();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsExhausted_NoLimit_ReturnsFalse()
    {
        // Arrange
        var file = CreateFile(downloadsLeft: null);

        // Act
        var result = file.IsExhausted();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsExhausted_DownloadsLeft_ReturnsFalse()
    {
        // Arrange
        var file = CreateFile(downloadsLeft: DownloadsLeft.Parse(1));

        // Act
        var result = file.IsExhausted();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsExhausted_NoDownloadsLeft_ReturnsTrue()
    {
        // Arrange
        var file = CreateFile(downloadsLeft: DownloadsLeft.Parse(0));

        // Act
        var result = file.IsExhausted();

        // Assert
        Assert.True(result);
    }

    private static SharedFile CreateFile(DateTimeOffset? expiresAt = null, DownloadsLeft? downloadsLeft = null) => new(
        ShareId.New(),
        FileName.Parse("report.pdf"),
        ContentType.Parse("application/pdf"),
        FileSize.Parse(1024),
        expiresAt,
        downloadsLeft: downloadsLeft);
}
