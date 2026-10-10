using Server.Domain.ValueObjects;

namespace Server.Application.GetFileInfo;

public sealed record GetFileInfoQuery(ShareId Id, Password? Password = null);
