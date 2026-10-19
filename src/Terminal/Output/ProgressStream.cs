namespace Mayfly.Terminal.Output;

internal sealed class ProgressStream(Stream inner, IProgress<long> progress) : Stream
{
    public override bool CanRead => inner.CanRead;

    public override bool CanSeek => inner.CanSeek;

    public override bool CanWrite => false;

    public override long Length => inner.Length;

    public override long Position
    {
        get => inner.Position;
        set
        {
            inner.Position = value;
            Report();
        }
    }

    public override int Read(byte[] buffer, int offset, int count) => Reported(inner.Read(buffer, offset, count));

    public override int Read(Span<byte> buffer) => Reported(inner.Read(buffer));

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        Reported(await inner.ReadAsync(buffer.AsMemory(offset, count), cancellationToken));

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
        Reported(await inner.ReadAsync(buffer, cancellationToken));

    public override long Seek(long offset, SeekOrigin origin)
    {
        var position = inner.Seek(offset, origin);
        Report();
        return position;
    }

    public override void Flush()
    {
    }

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            inner.Dispose();
        }

        base.Dispose(disposing);
    }

    private int Reported(int read)
    {
        Report();
        return read;
    }

    private void Report() => progress.Report(inner.Position);
}
