using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Mayfly.Client.Models;

namespace Mayfly.Client;

internal sealed partial class MayflyClient(HttpClient httpClient) : IMayflyClient
{
    private const string ExpiresAtHeader = "Mayfly-Expires-At";
    private const string PasswordHeader = "Mayfly-Password";

    public async Task<UploadedFile> UploadAsync(
        Stream content,
        string fileName,
        string? contentType = null,
        DateTimeOffset? expiresAt = null,
        string? password = null,
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

        if (password is not null)
        {
            form.Add(new StringContent(password), "password");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/files");
        request.Content = form;
        request.Headers.ExpectContinue = true;
        using var response = await httpClient.SendAsync(request, cancellationToken);

        await ThrowIfUnsuccessfulAsync(response, cancellationToken);

        return await TryReadJsonAsync(response.Content, JsonContext.Default.UploadedFile, cancellationToken)
               ?? throw UnexpectedResponse(response, "The server returned an unexpected response.");
    }

    public async Task<RemoteFileInfo?> GetInfoAsync(
        string id,
        string? password = null,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateFileRequest(HttpMethod.Head, id, password);
        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await ThrowIfUnsuccessfulAsync(response, cancellationToken);

        return ReadInfo(response);
    }

    public async Task<DownloadedFile?> DownloadAsync(
        string id,
        string? password = null,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateFileRequest(HttpMethod.Get, id, password);
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

    private static HttpRequestMessage CreateFileRequest(HttpMethod method, string id, string? password)
    {
        var request = new HttpRequestMessage(method, $"api/files/{Uri.EscapeDataString(id)}");
        if (password is not null)
        {
            request.Headers.Add(PasswordHeader, Convert.ToBase64String(Encoding.UTF8.GetBytes(password)));
        }

        return request;
    }

    private static RemoteFileInfo ReadInfo(HttpResponseMessage response)
    {
        var headers = response.Content.Headers;
        var disposition = headers.ContentDisposition;

        var fileName = disposition?.FileNameStar ?? disposition?.FileName?.Trim('"');
        if (fileName is null || headers.ContentType is null || headers.ContentLength is null)
        {
            throw UnexpectedResponse(response, "The server response does not describe the file.");
        }

        DateTimeOffset? expiresAt = null;
        if (response.Headers.TryGetValues(ExpiresAtHeader, out var values))
        {
            if (!DateTimeOffset.TryParse(values.First(), CultureInfo.InvariantCulture, out var parsed))
            {
                throw UnexpectedResponse(response, "The server response describes the file incorrectly.");
            }

            expiresAt = parsed;
        }

        return new RemoteFileInfo(fileName, headers.ContentType.ToString(), headers.ContentLength.Value, expiresAt);
    }

    private static async Task<T?> TryReadJsonAsync<T>(
        HttpContent content,
        JsonTypeInfo<T> typeInfo,
        CancellationToken cancellationToken)
        where T : class
    {
        try
        {
            return await content.ReadFromJsonAsync(typeInfo, cancellationToken);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            return null;
        }
    }

    private static MayflyApiException UnexpectedResponse(HttpResponseMessage response, string message) =>
        new(response.StatusCode, message, []);

    private static async Task ThrowIfUnsuccessfulAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var problem = await TryReadJsonAsync(response.Content, JsonContext.Default.ProblemDetails, cancellationToken);

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
