using System.Net.Http.Json;
using FootballManager.Application.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FootballManager.Api.IntegrationTests;

public sealed class HealthEndpointTests : IClassFixture<FootballManagerApiFactory>
{
    private readonly HttpClient client;

    public HealthEndpointTests(FootballManagerApiFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ReturnsOkStatus()
    {
        var response = await client.GetAsync("/api/health");

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<HealthStatusDto>();
        Assert.NotNull(payload);
        Assert.Equal("ok", payload.Status);
    }
}

public sealed class FootballManagerApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }
}
