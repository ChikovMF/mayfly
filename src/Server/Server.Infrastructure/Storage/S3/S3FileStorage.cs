using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Server.Domain.Abstractions;
using Server.Domain.ValueObjects;

namespace Server.Infrastructure.Storage.S3;

internal sealed class S3FileStorage(IAmazonS3 client, IOptions<S3Options> options) : IFileStorage
{
    public async Task SaveAsync(ShareId id, Stream content, FileSize size, CancellationToken cancellationToken)
    {
        var request = new PutObjectRequest
        {
            BucketName = options.Value.BucketName,
            Key = id.ToString(),
            InputStream = content,
            AutoCloseStream = false,
        };
        request.Headers.ContentLength = size.Bytes;

        await client.PutObjectAsync(request, cancellationToken);
    }

    public async Task<bool> ExistsAsync(ShareId id, CancellationToken cancellationToken)
    {
        try
        {
            await client.GetObjectMetadataAsync(options.Value.BucketName, id.ToString(), cancellationToken);
            return true;
        }
        catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task CopyToAsync(ShareId id, Stream destination, CancellationToken cancellationToken)
    {
        using var response = await client.GetObjectAsync(options.Value.BucketName, id.ToString(), cancellationToken);
        await response.ResponseStream.CopyToAsync(destination, cancellationToken);
    }

    public async Task DeleteAsync(ShareId id, CancellationToken cancellationToken)
    {
        await client.DeleteObjectAsync(options.Value.BucketName, id.ToString(), cancellationToken);
    }
}
