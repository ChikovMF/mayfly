namespace Server.Host.Endpoints;

internal sealed record FileUploadedResponse(string Id, DateTimeOffset? ExpiresAt);
