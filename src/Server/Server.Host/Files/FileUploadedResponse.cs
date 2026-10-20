namespace Server.Host.Files;

internal sealed record FileUploadedResponse(string Id, DateTimeOffset? ExpiresAt, int? MaxDownloads);
