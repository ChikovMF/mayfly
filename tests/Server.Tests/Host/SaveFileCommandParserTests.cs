using Server.Domain.ValueObjects;
using Server.Host.Files;

namespace Server.Tests.Host;

public class SaveFileCommandParserTests
{
    private static readonly RetentionOptions DefaultRetention = new();

    [Fact]
    public void TryParse_ValidForm_ReturnsTrueAndCommandWithFormValues()
    {
        // Arrange
        using var content = new MemoryStream();
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        // Act
        var success = SaveFileCommandParser.TryParse("report.pdf", "application/pdf", content, 5, expiresAt, "secret", 3, DefaultRetention, out var command, out var errors);

        // Assert
        Assert.True(success);
        Assert.Empty(errors);
        Assert.NotNull(command);
        Assert.Equal(FileName.Parse("report.pdf"), command.FileName);
        Assert.Equal(ContentType.Parse("application/pdf"), command.ContentType);
        Assert.Same(content, command.Content);
        Assert.Equal(FileSize.Parse(5), command.Size);
        Assert.Equal(expiresAt, command.ExpiresAt);
        Assert.Equal(Password.Parse("secret"), command.Password);
        Assert.Equal(DownloadsLeft.Parse(3), command.DownloadsLeft);
    }

    [Fact]
    public void TryParse_OnlyFile_ReturnsCommandWithoutOptionalValues()
    {
        // Act
        var success = SaveFileCommandParser.TryParse("report.pdf", "application/pdf", new MemoryStream(), 0, null, null, null, DefaultRetention, out var command, out _);

        // Assert
        Assert.True(success);
        Assert.NotNull(command);
        Assert.Null(command.ExpiresAt);
        Assert.Null(command.Password);
        Assert.Null(command.DownloadsLeft);
    }

    [Fact]
    public void TryParse_NoFile_ReturnsFileError()
    {
        // Act
        var success = SaveFileCommandParser.TryParse(null, null, null, 0, null, null, null, DefaultRetention, out var command, out var errors);

        // Assert
        Assert.False(success);
        Assert.Null(command);
        Assert.Equal(["file"], errors.Keys);
    }

    [Fact]
    public void TryParse_EmptyFileName_ReturnsFileError()
    {
        // Act
        var success = SaveFileCommandParser.TryParse("", "application/pdf", new MemoryStream(), 0, null, null, null, DefaultRetention, out _, out var errors);

        // Assert
        Assert.False(success);
        Assert.Equal(["file"], errors.Keys);
    }

    [Fact]
    public void TryParse_ExpirationInPast_ReturnsExpiresAtError()
    {
        // Act
        var success = SaveFileCommandParser.TryParse("report.pdf", "application/pdf", new MemoryStream(), 0, DateTimeOffset.UtcNow.AddMinutes(-1), null, null, DefaultRetention, out _, out var errors);

        // Assert
        Assert.False(success);
        Assert.Equal(["expiresAt"], errors.Keys);
    }

    [Fact]
    public void TryParse_NoExpirationWhenRequired_ReturnsExpiresAtError()
    {
        // Arrange
        var retention = new RetentionOptions { RequireExpiration = true };

        // Act
        var success = SaveFileCommandParser.TryParse("report.pdf", "application/pdf", new MemoryStream(), 0, null, null, null, retention, out _, out var errors);

        // Assert
        Assert.False(success);
        Assert.Equal(["expiresAt"], errors.Keys);
    }

    [Fact]
    public void TryParse_ExpirationWhenRequired_ReturnsTrue()
    {
        // Arrange
        var retention = new RetentionOptions { RequireExpiration = true };

        // Act
        var success = SaveFileCommandParser.TryParse("report.pdf", "application/pdf", new MemoryStream(), 0, DateTimeOffset.UtcNow.AddHours(1), null, null, retention, out _, out _);

        // Assert
        Assert.True(success);
    }

    [Fact]
    public void TryParse_ExpirationBeyondMaxLifetime_ReturnsExpiresAtError()
    {
        // Arrange
        var retention = new RetentionOptions { MaxLifetime = TimeSpan.FromDays(7) };

        // Act
        var success = SaveFileCommandParser.TryParse("report.pdf", "application/pdf", new MemoryStream(), 0, DateTimeOffset.UtcNow.AddDays(8), null, null, retention, out _, out var errors);

        // Assert
        Assert.False(success);
        Assert.Equal(["expiresAt"], errors.Keys);
    }

    [Fact]
    public void TryParse_ExpirationWithinMaxLifetime_ReturnsTrue()
    {
        // Arrange
        var retention = new RetentionOptions { MaxLifetime = TimeSpan.FromDays(7) };

        // Act
        var success = SaveFileCommandParser.TryParse("report.pdf", "application/pdf", new MemoryStream(), 0, DateTimeOffset.UtcNow.AddDays(6), null, null, retention, out _, out _);

        // Assert
        Assert.True(success);
    }

    [Fact]
    public void TryParse_NoExpirationWithOnlyMaxLifetime_ReturnsTrue()
    {
        // Arrange
        var retention = new RetentionOptions { MaxLifetime = TimeSpan.FromDays(7) };

        // Act
        var success = SaveFileCommandParser.TryParse("report.pdf", "application/pdf", new MemoryStream(), 0, null, null, null, retention, out var command, out _);

        // Assert
        Assert.True(success);
        Assert.NotNull(command);
        Assert.Null(command.ExpiresAt);
    }

    [Fact]
    public void TryParse_EmptyPassword_ReturnsPasswordError()
    {
        // Act
        var success = SaveFileCommandParser.TryParse("report.pdf", "application/pdf", new MemoryStream(), 0, null, "", null, DefaultRetention, out _, out var errors);

        // Assert
        Assert.False(success);
        Assert.Equal(["password"], errors.Keys);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void TryParse_MaxDownloadsNotPositive_ReturnsMaxDownloadsError(int maxDownloads)
    {
        // Act
        var success = SaveFileCommandParser.TryParse("report.pdf", "application/pdf", new MemoryStream(), 0, null, null, maxDownloads, DefaultRetention, out _, out var errors);

        // Assert
        Assert.False(success);
        Assert.Equal(["maxDownloads"], errors.Keys);
    }

    [Fact]
    public void TryParse_SeveralInvalidFields_ReturnsErrorForEach()
    {
        // Act
        var success = SaveFileCommandParser.TryParse(null, null, null, 0, DateTimeOffset.UtcNow.AddMinutes(-1), "", 0, DefaultRetention, out _, out var errors);

        // Assert
        Assert.False(success);
        Assert.Equal(["expiresAt", "file", "maxDownloads", "password"], errors.Keys.Order(StringComparer.Ordinal));
    }
}
