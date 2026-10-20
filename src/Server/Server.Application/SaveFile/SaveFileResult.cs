using Server.Domain.ValueObjects;

namespace Server.Application.SaveFile;

/// <param name="Sha256">The hash of the stored content, <see langword="null"/> if it could not be computed.</param>
public sealed record SaveFileResult(ShareId Id, Sha256? Sha256);
