using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Server.Application.SaveFile;
using Server.Domain.ValueObjects;

namespace Server.Host.Files;

internal static class SaveFileCommandParser
{
    private const string Sha256Prefix = "sha-256=:";

    public static bool TryParse(
        string? fileName,
        string? contentType,
        Stream? content,
        long size,
        DateTimeOffset? expiresAt,
        string? password,
        int? maxDownloads,
        string? reprDigest,
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

        Sha256? expectedSha256 = null;
        if (reprDigest is not null && !TryParseSha256(reprDigest, out expectedSha256))
        {
            errors["sha256"] = ["The digest is not valid."];
        }

        if (content is null || parsedFileName is null || parsedContentType is null || parsedSize is null || errors.Count > 0)
        {
            command = null;
            return false;
        }

        command = new SaveFileCommand(parsedFileName, parsedContentType, content, parsedSize, expiresAt, parsedPassword, downloadsLeft, expectedSha256);
        return true;
    }

    /// <returns><see langword="false"/> if the header has a malformed SHA-256 entry; <paramref name="sha256"/> is <see langword="null"/> if it has none.</returns>
    private static bool TryParseSha256(string reprDigest, out Sha256? sha256)
    {
        sha256 = null;
        foreach (var entry in reprDigest.Split(',', StringSplitOptions.TrimEntries))
        {
            if (!entry.StartsWith(Sha256Prefix, StringComparison.Ordinal))
            {
                continue;
            }

            if (entry.Length <= Sha256Prefix.Length || !entry.EndsWith(':'))
            {
                return false;
            }

            var bytes = new byte[entry.Length];
            if (!Convert.TryFromBase64Chars(entry.AsSpan(Sha256Prefix.Length, entry.Length - Sha256Prefix.Length - 1), bytes, out var written)
                || !Sha256.TryParse(bytes.AsSpan(0, written), out sha256))
            {
                return false;
            }

            return true;
        }

        return true;
    }
}
