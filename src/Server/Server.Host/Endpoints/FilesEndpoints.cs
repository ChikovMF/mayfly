using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Net.Http.Headers;
using Server.Application.GetFile;
using Server.Application.GetFileInfo;
using Server.Application.SaveFile;
using Server.Domain.ValueObjects;

namespace Server.Host.Endpoints;

internal static class FilesEndpoints
{
    private const string DefaultContentType = "application/octet-stream";
    private const string ExpiresAtHeader = "Mayfly-Expires-At";

    public static IEndpointRouteBuilder MapFilesEndpoints(this IEndpointRouteBuilder app)
    {
        var files = app.MapGroup("/api/files");

        files.MapPost("", UploadAsync).DisableAntiforgery();
        files.MapGet("/{id}", DownloadAsync);
        files.MapMethods("/{id}", [HttpMethods.Head], GetInfoAsync);

        return app;
    }

    private static async Task<IResult> UploadAsync(
        IFormFile? file,
        [FromForm] DateTimeOffset? expiresAt,
        [FromServices] ISaveFileHandler handler,
        [FromServices] IContentTypeProvider contentTypeProvider,
        CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return ValidationError("file", "A file is required.");
        }

        var errors = new List<string>();

        if (!FileName.TryParse(file.FileName, out var fileName))
        {
            errors.Add("The file name is not valid.");
        }

        if (!ContentType.TryParse(ResolveContentType(file, contentTypeProvider), out var contentType))
        {
            errors.Add("The content type is not valid.");
        }

        if (fileName is null || contentType is null)
        {
            return ValidationError("file", [.. errors]);
        }

        if (expiresAt <= DateTimeOffset.UtcNow)
        {
            return ValidationError("expiresAt", "The expiration must be in the future.");
        }

        await using var content = file.OpenReadStream();
        var id = await handler.HandleAsync(new SaveFileCommand(fileName, contentType, content, expiresAt), cancellationToken);

        return Results.Created($"/api/files/{id}", new FileUploadedResponse(id.ToString(), expiresAt));
    }

    private static async Task<IResult> DownloadAsync(
        string id,
        IGetFileHandler handler,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        if (!ShareId.TryParse(id, out var shareId))
        {
            return Results.NotFound();
        }

        var file = await handler.HandleAsync(new GetFileQuery(shareId), cancellationToken);
        if (file is null)
        {
            return Results.NotFound();
        }

        WriteFileHeaders(response, file.FileName, file.ContentType, file.Size, file.ExpiresAt);
        await file.CopyContentToAsync(response.Body, cancellationToken);

        return Results.Empty;
    }

    private static async Task<IResult> GetInfoAsync(
        string id,
        IGetFileInfoHandler handler,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        if (!ShareId.TryParse(id, out var shareId))
        {
            return Results.NotFound();
        }

        var result = await handler.HandleAsync(new GetFileInfoQuery(shareId), cancellationToken);
        switch (result)
        {
            case GetFileInfoResult.Found info:
                WriteFileHeaders(response, info.FileName, info.ContentType, info.Size, info.ExpiresAt);
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

    private static void WriteFileHeaders(
        HttpResponse response,
        FileName fileName,
        ContentType contentType,
        FileSize size,
        DateTimeOffset? expiresAt)
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
    }

    private static IResult ValidationError(string field, params string[] messages) =>
        Results.ValidationProblem(new Dictionary<string, string[]> { [field] = messages });
}
