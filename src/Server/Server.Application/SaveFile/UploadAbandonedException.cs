using Server.Domain.ValueObjects;

namespace Server.Application.SaveFile;

public sealed class UploadAbandonedException(ShareId id)
    : Exception($"Upload of file {id} was abandoned before it completed.");
