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
        Assert.False(string.IsNullOrWhiteSpace(createdGame.SeasonName));

        var dashboard = await client.GetFromJsonAsync<ClubDashboardDto>(
            $"/api/club/dashboard?gameId={createdGame.GameId}");

        Assert.NotNull(dashboard);
        Assert.Equal("Northbridge FC", dashboard.ClubName);
        Assert.Equal(createdGame.SeasonName, dashboard.SeasonName);
        Assert.Equal(20, dashboard.SquadSummary.TotalPlayers);
        Assert.Equal(11, dashboard.Lineup.StarterCount);
        Assert.False(string.IsNullOrWhiteSpace(dashboard.FeaturedPlayer.Name));
        Assert.NotNull(dashboard.NextFixture);
    }

    [Fact]
    public async Task SquadAndLineupEndpoints_ReturnPlayersAndPersistLineupChanges()
    {
        await using var factory = new FootballManagerApiFactory();
        using var client = factory.CreateClient();

        var createdGame = await CreateNorthbridgeGameAsync(client);

        var squad = await client.GetFromJsonAsync<List<SquadPlayerDto>>(
            $"/api/squad?gameId={createdGame.GameId}");

        Assert.NotNull(squad);
        Assert.Equal(20, squad.Count);
        Assert.Equal(11, squad.Count(player => player.IsStarter));
        Assert.All(squad, player => Assert.True(player.Attack > 0));

        var existingLineup = await client.GetFromJsonAsync<LineupEditorDto>(
            $"/api/lineup?gameId={createdGame.GameId}");

        Assert.NotNull(existingLineup);
        Assert.Equal(11, existingLineup.Lineup.StarterCount);
        Assert.NotEmpty(existingLineup.Formations);

        var featuredPlayer = squad.First(player => player.IsStarter);
        var playerDetail = await client.GetFromJsonAsync<PlayerDetailDto>(
            $"/api/player/{featuredPlayer.Id}?gameId={createdGame.GameId}");

        Assert.NotNull(playerDetail);
        Assert.Equal(featuredPlayer.Id, playerDetail.Id);
        Assert.True(playerDetail.IsStarter);

        var fourFourTwo = existingLineup.Formations.Single(formation => formation.Name == "4-4-2");
        var starterIds = squad
            .Where(player => player.Position == "Goalkeeper")
            .Take(1)
            .Concat(squad.Where(player => player.Position == "Defender").Take(4))
            .Concat(squad.Where(player => player.Position == "Midfielder").Take(4))
            .Concat(squad.Where(player => player.Position == "Forward").Take(2))
            .Select(player => player.Id)
            .ToArray();

        var updateResponse = await client.PostAsJsonAsync(
            $"/api/lineup?gameId={createdGame.GameId}",
            new UpdateLineupRequestDto(fourFourTwo.Id, starterIds));
        updateResponse.EnsureSuccessStatusCode();

        var updatedLineup = await updateResponse.Content.ReadFromJsonAsync<LineupDto>();

        Assert.NotNull(updatedLineup);
        Assert.Equal("4-4-2", updatedLineup.FormationName);
        Assert.Equal(11, updatedLineup.StarterCount);

        var refreshedDashboard = await client.GetFromJsonAsync<ClubDashboardDto>(
            $"/api/club/dashboard?gameId={createdGame.GameId}");

        Assert.NotNull(refreshedDashboard);
        Assert.Equal("4-4-2", refreshedDashboard.Lineup.FormationName);
        Assert.Equal(11, refreshedDashboard.Lineup.StarterCount);
    }

    private static async Task<CreateNewGameResponseDto> CreateNorthbridgeGameAsync(HttpClient client)
    {
        var clubs = await client.GetFromJsonAsync<List<ClubOptionDto>>("/api/game/clubs");
        var selectedClub = Assert.Single(clubs!, club => club.Name == "Northbridge FC");

        var createResponse = await client.PostAsJsonAsync("/api/game/new", new CreateNewGameRequestDto(selectedClub.Id));
        createResponse.EnsureSuccessStatusCode();

        var createdGame = await createResponse.Content.ReadFromJsonAsync<CreateNewGameResponseDto>();
        return Assert.IsType<CreateNewGameResponseDto>(createdGame);
    }
}
