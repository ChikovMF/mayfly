using Server.Domain.ValueObjects;

namespace Server.Application;

public sealed record SaveFileCommand(FileName FileName, ContentType ContentType, Stream Content);
