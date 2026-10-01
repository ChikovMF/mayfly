using Server.Domain.ValueObjects;

namespace Server.Application.GetFile;

public sealed record GetFileResult(FileName FileName, ContentType ContentType, Stream Content);
