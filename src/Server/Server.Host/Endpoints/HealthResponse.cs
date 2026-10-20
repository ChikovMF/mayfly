using System.Text.Json.Serialization;

namespace Server.Host.Endpoints;

internal sealed record HealthResponse(
    string Status,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyDictionary<string, string>? Checks);
