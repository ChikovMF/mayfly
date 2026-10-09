namespace Server.Application.DeleteExpiredFiles;

public sealed record DeleteExpiredFilesCommand(TimeSpan Lease, int MaxAttempts);
