namespace Server.Domain;

public sealed class SharedFile
{
    public SharedFile(ShareId id, FileName fileName, ContentType contentType, FileSize size)
    {
        Id = id;
        FileName = fileName;
        ContentType = contentType;
        Size = size;
    }

    public ShareId Id { get; }

    public FileName FileName { get; }

    public ContentType ContentType { get; }

    public FileSize Size { get; }
}
