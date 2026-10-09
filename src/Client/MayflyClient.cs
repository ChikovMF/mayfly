using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Mayfly.Client.Models;

namespace Mayfly.Client;

internal sealed partial class MayflyClient(HttpClient httpClient) : IMayflyClient
{
    private const string ExpiresAtHeader = "Mayfly-Expires-At";

    public async Task<UploadedFile> UploadAsync(
        Stream content,
        string fileName,
        string? contentType = null,
        DateTimeOffset? expiresAt = null,
        CancellationToken cancellationToken = default)
    {
        var mediaType = contentType is null ? null : MediaTypeHeaderValue.Parse(contentType);

        using var fileContent = new StreamContent(content);
        fileContent.Headers.ContentType = mediaType;

        using var form = new MultipartFormDataContent();
        form.Add(fileContent, "file", fileName);

        if (expiresAt.HasValue)
        {
            form.Add(new StringContent(expiresAt.Value.ToString("O", CultureInfo.InvariantCulture)), "expiresAt");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/files");
        request.Content = form;
        request.Headers.ExpectContinue = true;
        using var response = await httpClient.SendAsync(request, cancellationToken);

        await ThrowIfUnsuccessfulAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync(JsonContext.Default.UploadedFile, cancellationToken)
               ?? throw new InvalidOperationException("The server returned an empty response.");
    }

    public async Task<RemoteFileInfo?> GetInfoAsync(string id, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, FileUri(id));
        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await ThrowIfUnsuccessfulAsync(response, cancellationToken);

        return ReadInfo(response);
    }

    public async Task<DownloadedFile?> DownloadAsync(string id, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, FileUri(id));
        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        try
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                response.Dispose();
                return null;
            }

            await ThrowIfUnsuccessfulAsync(response, cancellationToken);

            var info = ReadInfo(response);
            var content = await response.Content.ReadAsStreamAsync(cancellationToken);

            return new DownloadedFile(info, content, response);
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }

    private static string FileUri(string id) => $"api/files/{Uri.EscapeDataString(id)}";

    private static RemoteFileInfo ReadInfo(HttpResponseMessage response)
    {
        var headers = response.Content.Headers;
        var disposition = headers.ContentDisposition;

        var fileName = disposition?.FileNameStar ?? disposition?.FileName?.Trim('"');
        if (fileName is null || headers.ContentType is null || headers.ContentLength is null)
        {
            throw new InvalidOperationException("The server response does not describe the file.");
        }

        DateTimeOffset? expiresAt = null;
        if (response.Headers.TryGetValues(ExpiresAtHeader, out var values))
        {
            expiresAt = DateTimeOffset.Parse(values.First(), CultureInfo.InvariantCulture);
        }

        return new RemoteFileInfo(fileName, headers.ContentType.ToString(), headers.ContentLength.Value, expiresAt);
    }

    private static async Task ThrowIfUnsuccessfulAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        ProblemDetails? problem = null;
        try
        {
            problem = await response.Content.ReadFromJsonAsync(JsonContext.Default.ProblemDetails, cancellationToken);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            // The body is not problem details (e.g. an error from a proxy); the status code is still reported.
        }

        var message = problem?.Detail
                      ?? problem?.Title
                      ?? $"The server responded with {(int)response.StatusCode} {response.StatusCode}.";

        throw new MayflyApiException(
            response.StatusCode,
            message,
            problem?.Errors?.Values.SelectMany(messages => messages).ToList() ?? []);
    }

    private sealed record ProblemDetails(string? Title, string? Detail, Dictionary<string, string[]>? Errors);

    [JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
    [JsonSerializable(typeof(UploadedFile))]
    [JsonSerializable(typeof(ProblemDetails))]
    private sealed partial class JsonContext : JsonSerializerContext;
}
