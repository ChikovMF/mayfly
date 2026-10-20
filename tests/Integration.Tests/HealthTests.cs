using System.Net;
using System.Net.Http.Json;

namespace Integration.Tests;

[Collection(MayflyCollection.Name)]
[Trait("Category", "Integration")]
public sealed class HealthTests(MayflyFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Health_AllDependenciesAvailable_ReturnsHealthy()
    {
        // Act
        using var response = await _client.GetAsync(new Uri("/health", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var health = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(health);
        Assert.Equal("Healthy", health.Status);
        Assert.Equal(new Dictionary<string, string> { ["database"] = "Healthy", ["storage"] = "Healthy" }, health.Checks);
    }

    [Fact]
    public async Task HealthLive_ReturnsOnlyStatus()
    {
        // Act
        using var response = await _client.GetAsync(new Uri("/health/live", UriKind.Relative));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("""{"status":"Healthy"}""", await response.Content.ReadAsStringAsync());
    }

    private sealed record HealthResponse(string Status, Dictionary<string, string> Checks);
}
