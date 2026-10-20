using Server.Domain.ValueObjects;

namespace Server.Application.SaveFile;

public sealed record SaveFileResult(ShareId Id, Sha256 Sha256);
