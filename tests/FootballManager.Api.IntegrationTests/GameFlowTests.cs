using System.Net.Http.Json;
using FootballManager.Application.Contracts;

namespace FootballManager.Api.IntegrationTests;

public sealed class GameFlowTests
{
    [Fact]
    public async Task GetAvailableClubs_ReturnsSeededTemplateClubs()
    {
        await using var factory = new FootballManagerApiFactory();
        using var client = factory.CreateClient();

        var clubs = await client.GetFromJsonAsync<List<ClubOptionDto>>("/api/game/clubs");

        Assert.NotNull(clubs);
        Assert.Equal(8, clubs.Count);
        Assert.All(clubs, club => Assert.False(string.IsNullOrWhiteSpace(club.Name)));
    }

    [Fact]
    public async Task CreateNewGame_AllowsDashboardLookupForSelectedClub()
    {
        await using var factory = new FootballManagerApiFactory();
        using var client = factory.CreateClient();

        var clubs = await client.GetFromJsonAsync<List<ClubOptionDto>>("/api/game/clubs");
        var selectedClub = Assert.Single(clubs!, club => club.Name == "Northbridge FC");

        var createResponse = await client.PostAsJsonAsync("/api/game/new", new CreateNewGameRequestDto(selectedClub.Id));
        createResponse.EnsureSuccessStatusCode();

        var createdGame = await createResponse.Content.ReadFromJsonAsync<CreateNewGameResponseDto>();
        Assert.NotNull(createdGame);
        Assert.Equal("Northbridge FC", createdGame.SelectedClub);

        var dashboard = await client.GetFromJsonAsync<ClubDashboardDto>(
            $"/api/club/dashboard?gameId={createdGame.GameId}");

        Assert.NotNull(dashboard);
        Assert.Equal("Northbridge FC", dashboard.ClubName);
        Assert.Equal(20, dashboard.SquadSummary.TotalPlayers);
        Assert.NotNull(dashboard.NextFixture);
    }
}
