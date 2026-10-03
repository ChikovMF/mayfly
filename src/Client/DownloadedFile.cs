namespace Mayfly.Client;

public sealed class DownloadedFile : IDisposable
{
    private readonly HttpResponseMessage _response;

    internal DownloadedFile(RemoteFileInfo info, Stream content, HttpResponseMessage response)
    {
        Info = info;
        Content = content;
        _response = response;
    }

    public RemoteFileInfo Info { get; }

    public Stream Content { get; }

    public void Dispose()
    {
        Content.Dispose();
        _response.Dispose();
    }
}
