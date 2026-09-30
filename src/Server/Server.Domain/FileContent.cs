namespace Server.Domain;

public sealed record FileContent(SharedFile File, Stream Stream);
