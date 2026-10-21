namespace Server.Domain.Abstractions;

public enum FileDeletionReason
{
    Expired,
    DownloadsExhausted,
    UploadAbandoned
}
