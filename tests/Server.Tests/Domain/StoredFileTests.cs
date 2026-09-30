using Server.Domain;

namespace Server.Tests.Domain;

public class StoredFileTests
{
    [Fact]
    public void Constructor_ValidValues_ExposesGivenValues()
    {
        // Arrange
        var id = ShareId.New();
        FileName.TryCreate("report.pdf", out var fileName);
        ContentType.TryCreate("application/pdf", out var contentType);
        FileSize.TryCreate(1024, out var size);

        // Act
        var file = new StoredFile(id, fileName!, contentType!, size!);

        // Assert
        Assert.Same(id, file.Id);
        Assert.Same(fileName, file.FileName);
        Assert.Same(contentType, file.ContentType);
        Assert.Same(size, file.Size);
    }
}
