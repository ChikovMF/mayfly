using Server.Domain.ValueObjects;

namespace Server.Application.GetFileInfo;

public sealed record GetFileInfoResult(FileName FileName, ContentType ContentType, FileSize Size, DateTimeOffset? ExpiresAt);
