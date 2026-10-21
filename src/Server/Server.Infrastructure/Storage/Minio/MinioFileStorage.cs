using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Server.Domain.Abstractions;
using Server.Domain.ValueObjects;

namespace Server.Infrastructure.Storage.Minio;

internal sealed class MinioFileStorage(IMinioClient client, IOptions<MinioOptions> options) : IFileStorage
{
    public async Task SaveAsync(ShareId id, Stream content, FileSize size, CancellationToken cancellationToken)
    {
        var args = new PutObjectArgs()
            .WithBucket(options.Value.BucketName)
            .WithObject(id.ToString())
            .WithStreamData(content)
            .WithObjectSize(size.Bytes);

        await client.PutObjectAsync(args, cancellationToken);
    }

    public async Task<bool> ExistsAsync(ShareId id, CancellationToken cancellationToken)
    {
        var args = new StatObjectArgs()
            .WithBucket(options.Value.BucketName)
            .WithObject(id.ToString());

        try
        {
            await client.StatObjectAsync(args, cancellationToken);
            return true;
        }
        catch (ObjectNotFoundException)
        {
            return false;
        }
    }

    public async Task CopyToAsync(ShareId id, Stream destination, CancellationToken cancellationToken)
    {
        var args = new GetObjectArgs()
            .WithBucket(options.Value.BucketName)
            .WithObject(id.ToString())
            .WithCallbackStream((source, ct) => source.CopyToAsync(destination, ct));

        await client.GetObjectAsync(args, cancellationToken);
    }

    public async Task DeleteAsync(ShareId id, CancellationToken cancellationToken)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(options.Value.BucketName)
            .WithObject(id.ToString());

        await client.RemoveObjectAsync(args, cancellationToken);
    }
}
