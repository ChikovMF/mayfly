using System.Security.Cryptography;
using Server.Domain.ValueObjects;

namespace Server.Application.SaveFile;

/// <summary>Passes a seekable stream through and computes the SHA-256 of what is read from it, from the start position to the end.</summary>
internal sealed class HashingStream : Stream
{
    private readonly Stream _inner;
    private readonly long _end;
    private readonly IncrementalHash _hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
    private long _hashedUpTo;
    private bool _broken;

    public HashingStream(Stream inner)
    {
        _inner = inner;
        _hashedUpTo = inner.Position;
        _end = inner.Length;
    }

    public override bool CanRead => _inner.CanRead;

    public override bool CanSeek => _inner.CanSeek;

    public override bool CanWrite => false;

    public override long Length => _inner.Length;

    public override long Position
    {
        get => _inner.Position;
        set => _inner.Position = value;
    }

    /// <returns>The hash, or <see langword="null"/> if the content was not read once through from start to end.</returns>
    public Sha256? GetHash()
    {
        if (_broken || _hashedUpTo != _end)
        {
            return null;
        }

        return Sha256.Parse(_hash.GetHashAndReset());
    }

    public override int Read(byte[] buffer, int offset, int count) => Hashed(buffer.AsSpan(offset, count), _inner.Read(buffer, offset, count));

    public override int Read(Span<byte> buffer) => Hashed(buffer, _inner.Read(buffer));

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var read = await _inner.ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
        return Hashed(buffer.AsSpan(offset, count), read);
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var read = await _inner.ReadAsync(buffer, cancellationToken);
        return Hashed(buffer.Span, read);
    }

    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);

    public override void Flush()
    {
    }

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _hash.Dispose();
        }

        base.Dispose(disposing);
    }

    private int Hashed(ReadOnlySpan<byte> buffer, int read)
    {
        if (read == 0 || _broken)
        {
            return read;
        }

        if (_inner.Position - read != _hashedUpTo)
        {
            _broken = true;
            return read;
        }

        _hash.AppendData(buffer[..read]);
        _hashedUpTo += read;
        return read;
    }
}
