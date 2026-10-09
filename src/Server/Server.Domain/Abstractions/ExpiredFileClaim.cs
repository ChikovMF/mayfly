using Server.Domain.ValueObjects;

namespace Server.Domain.Abstractions;

public sealed record ExpiredFileClaim(ShareId Id, int FailedAttempts);
