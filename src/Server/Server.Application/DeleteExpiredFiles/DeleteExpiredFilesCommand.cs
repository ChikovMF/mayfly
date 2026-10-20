namespace Server.Application.DeleteExpiredFiles;

public sealed record DeleteExpiredFilesCommand(TimeSpan Lease, TimeSpan ExhaustedFileDelay, int MaxAttempts);
