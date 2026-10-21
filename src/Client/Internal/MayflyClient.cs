using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Mayfly.Client.Exceptions;
using Mayfly.Client.Models;

namespace Mayfly.Client.Internal;

internal sealed partial class MayflyClient(HttpClient httpClient) : IMayflyClient
{
    private const string ExpiresAtHeader = "Mayfly-Expires-At";
    private const string MaxDownloadsHeader = "Mayfly-Max-Downloads";
    private const string DownloadsLeftHeader = "Mayfly-Downloads-Left";
    private const string ReprDigestHeader = "Repr-Digest";
    private const string Sha256Prefix = "sha-256=:";
    private const string PasswordHeader = "Mayfly-Password";

    public async Task<UploadedFile> UploadAsync(
        Stream content,
        string fileName,
        string? contentType = null,
        DateTimeOffset? expiresAt = null,
        string? password = null,
        int? maxDownloads = null,
        CancellationToken cancellationToken = default)
    {
        if (!content.CanSeek)
        {
            throw new ArgumentException("The content stream must support seeking.", nameof(content));
        }

        var start = content.Position;
        var hash = await SHA256.HashDataAsync(content, cancellationToken);
        content.Position = start;

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/files");
        request.Content = new StreamContent(content);
        request.Content.Headers.ContentType = contentType is null ? null : MediaTypeHeaderValue.Parse(contentType);
        request.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileNameStar = fileName };
        request.Headers.ExpectContinue = true;
        request.Headers.Add(ReprDigestHeader, $"{Sha256Prefix}{Convert.ToBase64String(hash)}:");

        if (expiresAt.HasValue)
        {
            request.Headers.Add(ExpiresAtHeader, expiresAt.Value.ToString("O", CultureInfo.InvariantCulture));
        }

        if (password is not null)
        {
            request.Headers.Add(PasswordHeader, Convert.ToBase64String(Encoding.UTF8.GetBytes(password)));
        }

        if (maxDownloads.HasValue)
        {
            request.Headers.Add(MaxDownloadsHeader, maxDownloads.Value.ToString(CultureInfo.InvariantCulture));
        }

        using var response = await httpClient.SendAsync(request, cancellationToken);

        await ThrowIfUnsuccessfulAsync(response, cancellationToken);

        return await TryReadJsonAsync<UploadedFile>(response.Content, JsonContext.Default.UploadedFile, cancellationToken)
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

        int? downloadsLeft = null;
        if (response.Headers.TryGetValues(DownloadsLeftHeader, out var downloadsLeftValues))
        {
            if (!int.TryParse(downloadsLeftValues.First(), NumberStyles.None, CultureInfo.InvariantCulture, out var parsed))
            {
                throw UnexpectedResponse(response, "The server response describes the file incorrectly.");
            }

            downloadsLeft = parsed;
        }

        return new RemoteFileInfo(
            fileName,
            headers.ContentType.ToString(),
            headers.ContentLength.Value,
            expiresAt,
            downloadsLeft,
            ReadSha256(response));
    }

    private static string ReadSha256(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues(ReprDigestHeader, out var values))
        {
            var entry = values.SelectMany(value => value.Split(',', StringSplitOptions.TrimEntries))
                .FirstOrDefault(e => e.StartsWith(Sha256Prefix, StringComparison.Ordinal));
            if (entry is not null && entry.Length > Sha256Prefix.Length && entry.EndsWith(':'))
            {
                var bytes = new byte[entry.Length];
                if (Convert.TryFromBase64Chars(entry.AsSpan(Sha256Prefix.Length, entry.Length - Sha256Prefix.Length - 1), bytes, out var written))
                {
                    return Convert.ToHexStringLower(bytes.AsSpan(0, written));
                }
            }
        }

        throw UnexpectedResponse(response, "The server response does not contain the hash of the file.");
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

        var problem = await TryReadJsonAsync<ProblemDetails>(response.Content, JsonContext.Default.ProblemDetails, cancellationToken);

        var message = problem?.Detail
                      ?? problem?.Title
                      ?? $"The server responded with {(int)response.StatusCode} {response.StatusCode}.";

        IReadOnlyList<string> errors = problem?.Errors?.Values.SelectMany(messages => messages).ToList() ?? [];

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new MayflyRateLimitException(message, errors, response.Headers.RetryAfter?.Delta);
        }

        throw new MayflyApiException(response.StatusCode, message, errors);
    }

    private sealed record ProblemDetails(string? Title, string? Detail, Dictionary<string, string[]>? Errors);

    [JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
    [JsonSerializable(typeof(UploadedFile))]
    [JsonSerializable(typeof(ProblemDetails))]
    private sealed partial class JsonContext : JsonSerializerContext;
}
