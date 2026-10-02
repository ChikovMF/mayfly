using Microsoft.Net.Http.Headers;
using Server.Application.GetFile;
using Server.Application.GetFileInfo;
using Server.Application.SaveFile;
using Server.Domain.ValueObjects;

namespace Server.Host.Endpoints;

internal static class FilesEndpoints
{
    private const string DefaultContentType = "application/octet-stream";

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
        ISaveFileHandler handler,
        CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return ValidationError("A file is required.");
        }

        var errors = new List<string>();

        if (!FileName.TryParse(file.FileName, out var fileName))
        {
            errors.Add("The file name is not valid.");
        }

        var contentTypeValue = string.IsNullOrEmpty(file.ContentType) ? DefaultContentType : file.ContentType;
        if (!ContentType.TryParse(contentTypeValue, out var contentType))
        {
            errors.Add("The content type is not valid.");
        }

        if (fileName is null || contentType is null)
        {
            return ValidationError([.. errors]);
        }

        await using var content = file.OpenReadStream();
        var id = await handler.HandleAsync(new SaveFileCommand(fileName, contentType, content), cancellationToken);

        return Results.Created($"/api/files/{id}", new FileUploadedResponse(id.ToString()));
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

        WriteFileHeaders(response, file.FileName, file.ContentType, file.Size);
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

        var info = await handler.HandleAsync(new GetFileInfoQuery(shareId), cancellationToken);
        if (info is null)
        {
            return Results.NotFound();
        }

        WriteFileHeaders(response, info.FileName, info.ContentType, info.Size);

        return Results.Empty;
    }

    private static void WriteFileHeaders(HttpResponse response, FileName fileName, ContentType contentType, FileSize size)
    {
        var contentDisposition = new ContentDispositionHeaderValue("attachment");
        contentDisposition.SetHttpFileName(fileName.ToString());

        response.ContentType = contentType.ToString();
        response.ContentLength = size.Bytes;
        response.Headers.ContentDisposition = contentDisposition.ToString();
    }

    private static IResult ValidationError(params string[] messages) =>
        Results.ValidationProblem(new Dictionary<string, string[]> { ["file"] = messages });
}
