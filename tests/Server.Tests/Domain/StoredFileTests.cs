using Server.Domain;

namespace Server.Tests.Domain;

public class StoredFileTests
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
        var file = new StoredFile(id, fileName, contentType, size);

        // Assert
        Assert.Same(id, file.Id);
        Assert.Same(fileName, file.FileName);
        Assert.Same(contentType, file.ContentType);
        Assert.Same(size, file.Size);
    }
}
