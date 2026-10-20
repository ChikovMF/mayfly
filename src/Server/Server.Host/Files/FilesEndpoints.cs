using System.Globalization;
using System.Text;
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
    private const string ExpiresAtHeader = "Mayfly-Expires-At";
    private const string DownloadsLeftHeader = "Mayfly-Downloads-Left";
    internal const string PasswordHeader = "Mayfly-Password";

    public static IEndpointRouteBuilder MapFilesEndpoints(this IEndpointRouteBuilder app)
    {
        var files = app.MapGroup("/api/files");

        files.MapPost("", UploadAsync).DisableAntiforgery();
        files.MapGet("/{id}", DownloadAsync).RequireRateLimiting(PasswordRateLimiterPolicy.Name);
        files.MapMethods("/{id}", [HttpMethods.Head], GetInfoAsync).RequireRateLimiting(PasswordRateLimiterPolicy.Name);

        return app;
    }

    private static async Task<IResult> UploadAsync(
        IFormFile? file,
        [FromForm] DateTimeOffset? expiresAt,
        [FromForm] string? password,
        [FromForm] int? maxDownloads,
        [FromServices] ISaveFileHandler handler,
        [FromServices] IContentTypeProvider contentTypeProvider,
        [FromServices] IOptions<RetentionOptions> retention,
        CancellationToken cancellationToken)
    {
        await using var content = file?.OpenReadStream();
        var contentType = file is null ? null : ResolveContentType(file, contentTypeProvider);
        if (!SaveFileCommandParser.TryParse(
                file?.FileName,
                contentType,
                content,
                expiresAt,
                password,
                maxDownloads,
                retention.Value,
                out var command,
                out var errors))
        {
            return Results.ValidationProblem(errors);
        }

        var id = await handler.HandleAsync(command, cancellationToken);

        return Results.Created($"/api/files/{id}", new FileUploadedResponse(id.ToString(), expiresAt, maxDownloads));
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
                WriteFileHeaders(response, file.FileName, file.ContentType, file.Size, file.ExpiresAt, file.DownloadsLeft);
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
                WriteFileHeaders(response, info.FileName, info.ContentType, info.Size, info.ExpiresAt, info.DownloadsLeft);
                return Results.Empty;
            case GetFileInfoResult.AccessDenied:
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            default:
                return Results.NotFound();
        }
    }

    private static string ResolveContentType(IFormFile file, IContentTypeProvider contentTypeProvider)
    {
        if (!string.IsNullOrEmpty(file.ContentType))
        {
            return file.ContentType;
        }

        return contentTypeProvider.TryGetContentType(file.FileName, out var contentType) ? contentType : DefaultContentType;
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
        DownloadsLeft? downloadsLeft)
    {
        var contentDisposition = new ContentDispositionHeaderValue("attachment");
        contentDisposition.SetHttpFileName(fileName.ToString());

        response.ContentType = contentType.ToString();
        response.ContentLength = size.Bytes;
        response.Headers.ContentDisposition = contentDisposition.ToString();

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
