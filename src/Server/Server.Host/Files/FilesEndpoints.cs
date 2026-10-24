using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Server.Application.GetFile;
using Server.Application.GetFileInfo;
using Server.Application.SaveFile;
using Server.Domain.ValueObjects;
using Server.Host.RateLimiting;

namespace Server.Host.Files;

internal static class FilesEndpoints
{
    private const string DefaultContentType = "application/octet-stream";
    private const string ContentDispositionHeader = "Content-Disposition";
    private const string ContentTypeHeader = "Content-Type";
    internal const string ExpiresAtHeader = "Mayfly-Expires-At";
    internal const string MaxDownloadsHeader = "Mayfly-Max-Downloads";
    internal const string DownloadsLeftHeader = "Mayfly-Downloads-Left";
    internal const string ReprDigestHeader = "Repr-Digest";
    internal const string PasswordHeader = "Mayfly-Password";

    public static IEndpointRouteBuilder MapFilesEndpoints(this IEndpointRouteBuilder app)
    {
        var files = app.MapGroup("/api/files");

        files.MapPost("", UploadAsync);
        files.MapGet("/{id}", DownloadAsync).RequireRateLimiting(PasswordRateLimiterPolicy.Name);
        files.MapMethods("/{id}", [HttpMethods.Head], GetInfoAsync).RequireRateLimiting(PasswordRateLimiterPolicy.Name);

        return app;
    }

    private static async Task<IResult> UploadAsync(
        HttpRequest request,
        [FromHeader(Name = ContentDispositionHeader)] string? contentDisposition,
        [FromHeader(Name = ContentTypeHeader)] string? contentType,
        [FromHeader(Name = ExpiresAtHeader)] DateTimeOffset? expiresAt,
        [FromHeader(Name = PasswordHeader)] string? password,
        [FromHeader(Name = MaxDownloadsHeader)] int? maxDownloads,
        [FromHeader(Name = ReprDigestHeader)] string? reprDigest,
        [FromServices] ISaveFileHandler handler,
        [FromServices] IContentTypeProvider contentTypeProvider,
        [FromServices] IOptions<RetentionOptions> retention,
        CancellationToken cancellationToken)
    {
        if (request.ContentLength is null)
        {
            return Results.StatusCode(StatusCodes.Status411LengthRequired);
        }

        var maxSize = request.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>()?.MaxRequestBodySize;
        if (request.ContentLength > maxSize)
        {
            return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
        }

        var fileName = ReadFileName(contentDisposition);
        if (!SaveFileCommandParser.TryParse(
                fileName,
                ResolveContentType(fileName, contentType, contentTypeProvider),
                request.Body,
                request.ContentLength.Value,
                expiresAt,
                DecodePassword(password),
                maxDownloads,
                reprDigest,
                retention.Value,
                out var command,
                out var errors))
        {
            return Results.ValidationProblem(errors);
        }

        var result = await handler.HandleAsync(command, cancellationToken);
        switch (result)
        {
            case SaveFileResult.Saved saved:
                return Results.Created($"/api/files/{saved.Id}", new FileUploadedResponse(saved.Id.ToString(), expiresAt, maxDownloads, saved.Sha256.Hex));
            default:
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["sha256"] = ["The content does not match the digest."] });
        }
    }

    private static async Task<IResult> DownloadAsync(
        string id,
        [FromHeader(Name = PasswordHeader)] string? password,
        IGetFileHandler handler,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        if (!ShareId.TryParse(id, out var shareId))
        {
            return Results.NotFound();
        }

        var result = await handler.HandleAsync(new GetFileQuery(shareId, ReadPassword(password)), cancellationToken);
        switch (result)
        {
            case GetFileResult.Found file:
                WriteFileHeaders(response, file.FileName, file.ContentType, file.Size, file.ExpiresAt, file.DownloadsLeft, file.Sha256);
                await file.CopyContentToAsync(response.Body, cancellationToken);
                return Results.Empty;
            case GetFileResult.AccessDenied:
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            default:
                return Results.NotFound();
        }
    }

    private static async Task<IResult> GetInfoAsync(
        string id,
        [FromHeader(Name = PasswordHeader)] string? password,
        IGetFileInfoHandler handler,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        if (!ShareId.TryParse(id, out var shareId))
        {
            return Results.NotFound();
        }

        var result = await handler.HandleAsync(new GetFileInfoQuery(shareId, ReadPassword(password)), cancellationToken);
        switch (result)
        {
            case GetFileInfoResult.Found info:
                WriteFileHeaders(response, info.FileName, info.ContentType, info.Size, info.ExpiresAt, info.DownloadsLeft, info.Sha256);
                return Results.Empty;
            case GetFileInfoResult.AccessDenied:
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            default:
                return Results.NotFound();
        }
    }

    private static string? ReadFileName(string? contentDisposition)
    {
        if (!ContentDispositionHeaderValue.TryParse(contentDisposition, out var disposition))
        {
            return null;
        }

        var fileName = disposition.FileNameStar.HasValue ? disposition.FileNameStar : disposition.FileName;
        return HeaderUtilities.RemoveQuotes(fileName).Value;
    }

    private static string? ResolveContentType(string? fileName, string? contentType, IContentTypeProvider contentTypeProvider)
    {
        if (!string.IsNullOrEmpty(contentType))
        {
            return contentType;
        }

        return fileName is not null && contentTypeProvider.TryGetContentType(fileName, out var resolved) ? resolved : DefaultContentType;
    }

    private static string? DecodePassword(string? header)
    {
        if (header is null)
        {
            return null;
        }

        var bytes = new byte[header.Length];
        if (!Convert.TryFromBase64String(header, bytes, out var written))
        {
            throw new BadHttpRequestException($"The {PasswordHeader} header is not valid base64.");
        }

        return Encoding.UTF8.GetString(bytes, 0, written);
    }

    private static Password? ReadPassword(string? header)
    {
        if (header is null)
        {
            return null;
        }

        var bytes = new byte[header.Length];
        if (!Convert.TryFromBase64String(header, bytes, out var written))
        {
            return null;
        }

        return Password.TryParse(Encoding.UTF8.GetString(bytes, 0, written), out var password) ? password : null;
    }

    private static void WriteFileHeaders(
        HttpResponse response,
        FileName fileName,
        ContentType contentType,
        FileSize size,
        DateTimeOffset? expiresAt,
        DownloadsLeft? downloadsLeft,
        Sha256 sha256)
    {
        var contentDisposition = new ContentDispositionHeaderValue("attachment");
        contentDisposition.SetHttpFileName(fileName.ToString());

        response.ContentType = contentType.ToString();
        response.ContentLength = size.Bytes;
        response.Headers.ContentDisposition = contentDisposition.ToString();
        response.Headers[ReprDigestHeader] = $"sha-256=:{Convert.ToBase64String(sha256.ToBytes())}:";

        if (expiresAt.HasValue)
        {
            response.Headers[ExpiresAtHeader] = expiresAt.Value.UtcDateTime.ToString("O", CultureInfo.InvariantCulture);
        }

        if (downloadsLeft != null)
        {
            response.Headers[DownloadsLeftHeader] = downloadsLeft.Count.ToString(CultureInfo.InvariantCulture);
        }
    }
}
