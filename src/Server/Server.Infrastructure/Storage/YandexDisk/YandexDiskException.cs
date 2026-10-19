using System.Net;

namespace Server.Infrastructure.Storage.YandexDisk;

internal sealed class YandexDiskException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
