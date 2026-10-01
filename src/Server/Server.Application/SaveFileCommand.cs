using Server.Domain;

namespace Server.Application;

public sealed record SaveFileCommand(FileName FileName, ContentType ContentType, Stream Content);
