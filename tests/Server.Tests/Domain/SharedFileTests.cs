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
}
