namespace Server.Domain;

public sealed class FileContent(SharedFile file, Stream stream) : IAsyncDisposable
{
    public SharedFile File { get; } = file;

    public Stream Stream { get; } = stream;

    public ValueTask DisposeAsync() => Stream.DisposeAsync();
}
