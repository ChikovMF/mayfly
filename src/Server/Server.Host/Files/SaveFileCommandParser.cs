using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Server.Application.SaveFile;
using Server.Domain.ValueObjects;

namespace Server.Host.Files;

internal static class SaveFileCommandParser
{
    public static bool TryParse(
        string? fileName,
        string? contentType,
        Stream? content,
        long size,
        DateTimeOffset? expiresAt,
        string? password,
        int? maxDownloads,
        RetentionOptions retention,
        [NotNullWhen(true)] out SaveFileCommand? command,
        out Dictionary<string, string[]> errors)
    {
        errors = [];

        FileName? parsedFileName = null;
        ContentType? parsedContentType = null;
        FileSize? parsedSize = null;
        if (content is null)
        {
            errors["file"] = ["A file is required."];
        }
        else
        {
            var fileErrors = new List<string>();

            if (!FileName.TryParse(fileName, out parsedFileName))
            {
                fileErrors.Add("The file name is not valid.");
            }

            if (!ContentType.TryParse(contentType, out parsedContentType))
            {
                fileErrors.Add("The content type is not valid.");
            }

            if (!FileSize.TryParse(size, out parsedSize))
            {
                fileErrors.Add("The file size is not valid.");
            }

            if (fileErrors.Count > 0)
            {
                errors["file"] = [.. fileErrors];
            }
        }

        if (expiresAt.HasValue)
        {
            var lifetime = expiresAt.Value - DateTimeOffset.UtcNow;
            if (lifetime <= TimeSpan.Zero)
            {
                errors["expiresAt"] = ["The expiration must be in the future."];
            }
            else if (lifetime > retention.MaxLifetime)
            {
                errors["expiresAt"] = [$"The expiration must be at most {retention.MaxLifetime.Value.ToString("c", CultureInfo.InvariantCulture)} from now."];
            }
        }
        else if (retention.RequireExpiration)
        {
            errors["expiresAt"] = ["An expiration is required."];
        }

        Password? parsedPassword = null;
        if (password is not null && !Password.TryParse(password, out parsedPassword))
        {
            errors["password"] = ["The password is not valid."];
        }

        DownloadsLeft? downloadsLeft = null;
        if (maxDownloads < 1)
        {
            errors["maxDownloads"] = ["The number of downloads must be at least one."];
        }
        else if (maxDownloads.HasValue)
        {
            downloadsLeft = DownloadsLeft.Parse(maxDownloads.Value);
        }

        if (content is null || parsedFileName is null || parsedContentType is null || parsedSize is null || errors.Count > 0)
        {
            command = null;
            return false;
        }

        command = new SaveFileCommand(parsedFileName, parsedContentType, content, parsedSize, expiresAt, parsedPassword, downloadsLeft);
        return true;
    }
}
