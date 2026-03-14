using System.Net.Http.Json;
using FootballManager.Application.Contracts;

namespace FootballManager.Api.IntegrationTests;

public sealed class HealthEndpointTests
{
    [Fact]
    public async Task GetHealth_ReturnsOkStatus()
    {
        await using var factory = new FootballManagerApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/health");

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<HealthStatusDto>();
        Assert.NotNull(payload);
        Assert.Equal("ok", payload.Status);
    }
}
