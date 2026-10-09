namespace Mayfly.Client.Models;

public sealed record RemoteFileInfo(string FileName, string ContentType, long Size, DateTimeOffset? ExpiresAt);
