namespace Mayfly.Client.Models;

public sealed record UploadedFile(string Id, DateTimeOffset? ExpiresAt);
