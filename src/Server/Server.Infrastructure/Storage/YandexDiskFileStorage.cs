using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Server.Domain.Abstractions;
using Server.Domain.ValueObjects;

namespace Server.Infrastructure.Storage;

internal sealed class YandexDiskFileStorage(HttpClient client, IOptions<YandexDiskOptions> options) : IFileStorage
{
    public static readonly Uri ApiAddress = new("https://cloud-api.yandex.net/v1/disk/");

    public async Task SaveAsync(ShareId id, Stream content, CancellationToken cancellationToken)
    {
        var uploadUrl = await GetLinkAsync($"resources/upload?path={EscapedPath(id)}&overwrite=false", cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Put, uploadUrl);
        request.Content = new StreamContent(content);
        using var response = await client.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<bool> ExistsAsync(ShareId id, CancellationToken cancellationToken)
    {
        using var request = CreateApiRequest(HttpMethod.Get, $"resources?path={EscapedPath(id)}&fields=type");
        using var response = await client.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        await EnsureSuccessAsync(response, cancellationToken);
        return true;
    }

    public async Task CopyToAsync(ShareId id, Stream destination, CancellationToken cancellationToken)
    {
        var downloadUrl = await GetLinkAsync($"resources/download?path={EscapedPath(id)}", cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
        request.Headers.Authorization = CreateAuthorization();
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        await response.Content.CopyToAsync(destination, cancellationToken);
    }

    public async Task DeleteAsync(ShareId id, CancellationToken cancellationToken)
    {
        using var request = CreateApiRequest(HttpMethod.Delete, $"resources?path={EscapedPath(id)}&permanently=true");
        using var response = await client.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return;
        }

        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static string EscapedPath(ShareId id) => Uri.EscapeDataString($"app:/{id}");

    private HttpRequestMessage CreateApiRequest(HttpMethod method, string relativeUrl)
    {
        var request = new HttpRequestMessage(method, new Uri(relativeUrl, UriKind.Relative));
        request.Headers.Authorization = CreateAuthorization();
        return request;
    }

    private AuthenticationHeaderValue CreateAuthorization() => new("OAuth", options.Value.OAuthToken);

    private async Task<Uri> GetLinkAsync(string relativeUrl, CancellationToken cancellationToken)
    {
        using var request = CreateApiRequest(HttpMethod.Get, relativeUrl);
        using var response = await client.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var link = await response.Content.ReadFromJsonAsync<DiskLink>(cancellationToken);
        if (!Uri.TryCreate(link?.Href, UriKind.Absolute, out var href))
        {
            throw new YandexDiskException(response.StatusCode, "Yandex Disk returned no link.");
        }

        return href;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var error = await TryReadErrorAsync(response, cancellationToken);
        var details = error?.Error is null ? "" : $" ({error.Error}: {error.Description})";
        throw new YandexDiskException(response.StatusCode, $"Yandex Disk responded with {(int)response.StatusCode} {response.StatusCode}{details}.");
    }

    private static async Task<DiskError?> TryReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<DiskError>(cancellationToken);
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException)
        {
            return null;
        }
    }

    private sealed record DiskLink(string? Href);

    private sealed record DiskError(string? Error, string? Description);
}
