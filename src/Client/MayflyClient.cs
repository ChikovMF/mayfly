using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Mayfly.Client;

internal sealed class MayflyClient(HttpClient httpClient) : IMayflyClient
{
    public async Task<UploadedFile> UploadAsync(
        Stream content,
        string fileName,
        string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        var mediaType = contentType is null ? null : MediaTypeHeaderValue.Parse(contentType);

        using var fileContent = new StreamContent(content) { Headers = { ContentType = mediaType } };

        using var form = new MultipartFormDataContent { { fileContent, "file", fileName } };
        using var response = await httpClient.PostAsync("api/files", form, cancellationToken);

        await ThrowIfUnsuccessfulAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<UploadedFile>(cancellationToken)
               ?? throw new InvalidOperationException("The server returned an empty response.");
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
            problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);
        }
        catch (Exception ex) when (ex is System.Text.Json.JsonException or NotSupportedException)
        {
            // The body is not problem details (e.g. an error from a proxy); the status code is still reported.
        }

        throw new MayflyApiException(
            response.StatusCode,
            problem?.Detail ?? problem?.Title ?? $"The server responded with {(int)response.StatusCode} {response.StatusCode}.",
            problem?.Errors?.Values.SelectMany(messages => messages).ToList() ?? []);
    }

    private sealed record ProblemDetails(string? Title, string? Detail, Dictionary<string, string[]>? Errors);
}
