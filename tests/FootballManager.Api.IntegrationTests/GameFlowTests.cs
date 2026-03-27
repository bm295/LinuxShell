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
        Assert.Equal("Arsenal", clubs[0].Name);
        Assert.DoesNotContain(clubs, club => club.Name == "Cedar City FC");
        Assert.All(clubs, club => Assert.False(string.IsNullOrWhiteSpace(club.Name)));
    }

    [Fact]
    public async Task CreateNewGame_AllowsDashboardLookupForSelectedClub()
    {
        await using var factory = new FootballManagerApiFactory();
        using var client = factory.CreateClient();

        var clubs = await client.GetFromJsonAsync<List<ClubOptionDto>>("/api/game/clubs");
        var selectedClub = Assert.Single(clubs!, club => club.Name == "Arsenal");

        var createResponse = await client.PostAsJsonAsync("/api/game/new", new CreateNewGameRequestDto(selectedClub.Id));
        createResponse.EnsureSuccessStatusCode();

        var createdGame = await createResponse.Content.ReadFromJsonAsync<CreateNewGameResponseDto>();
        Assert.NotNull(createdGame);
        Assert.Equal("Arsenal", createdGame.SelectedClub);
        Assert.Equal("Season 1", createdGame.SeasonName);

        var dashboard = await client.GetFromJsonAsync<ClubDashboardDto>(
            $"/api/club/dashboard?gameId={createdGame.GameId}");

        Assert.NotNull(dashboard);
        Assert.Equal("Arsenal", dashboard.ClubName);
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

        var createdGame = await CreateArsenalGameAsync(client);

        var squad = await client.GetFromJsonAsync<List<SquadPlayerDto>>(
            $"/api/squad?gameId={createdGame.GameId}");

        Assert.NotNull(squad);
        Assert.Equal(20, squad.Count);
        Assert.Equal(11, squad.Count(player => player.IsStarter));
        Assert.All(squad, player => Assert.True(player.Attack > 0));
        Assert.All(squad, player => Assert.InRange(player.Age, 15, 39));
        Assert.All(squad, player => Assert.InRange(player.SquadNumber, 1, 99));
        Assert.Single(squad, player => player.IsCaptain);
        Assert.Contains(squad, player => player.SquadNumber == 4 && player.Name == "Cesc Fàbregas" && player.Age == 17 && player.IsCaptain);

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
        Assert.InRange(playerDetail.Age, 15, 39);

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

        var reopenedLineup = await client.GetFromJsonAsync<LineupEditorDto>(
            $"/api/lineup?gameId={createdGame.GameId}");

        Assert.NotNull(reopenedLineup);
        Assert.Equal("4-4-2", reopenedLineup.Lineup.FormationName);
        Assert.Equal(fourFourTwo.Id, reopenedLineup.Lineup.FormationId);
    }

    [Fact]
    public async Task PlayerEndpoint_UpdatesPlayerPositionAndKeepsLineupAvailable()
    {
        await using var factory = new FootballManagerApiFactory();
        using var client = factory.CreateClient();

        var createdGame = await CreateArsenalGameAsync(client);
        var initialSquad = await client.GetFromJsonAsync<List<SquadPlayerDto>>(
            $"/api/squad?gameId={createdGame.GameId}");

        Assert.NotNull(initialSquad);
        var player = Assert.IsType<SquadPlayerDto>(initialSquad.FirstOrDefault(candidate => candidate.Position == "Forward"));
        var initialDefenderCount = initialSquad.Count(candidate => candidate.Position == "Defender");
        var initialForwardCount = initialSquad.Count(candidate => candidate.Position == "Forward");

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/player/{player.Id}/position?gameId={createdGame.GameId}",
            new UpdatePlayerPositionRequestDto("Defender"));
        updateResponse.EnsureSuccessStatusCode();

        var updatedPlayer = await updateResponse.Content.ReadFromJsonAsync<PlayerDetailDto>();
        Assert.NotNull(updatedPlayer);
        Assert.Equal(player.Id, updatedPlayer.Id);
        Assert.Equal("Defender", updatedPlayer.Position);

        var refreshedSquad = await client.GetFromJsonAsync<List<SquadPlayerDto>>(
            $"/api/squad?gameId={createdGame.GameId}");
        var refreshedLineup = await client.GetFromJsonAsync<LineupEditorDto>(
            $"/api/lineup?gameId={createdGame.GameId}");

        Assert.NotNull(refreshedSquad);
        Assert.Equal(initialDefenderCount + 1, refreshedSquad.Count(candidate => candidate.Position == "Defender"));
        Assert.Equal(initialForwardCount - 1, refreshedSquad.Count(candidate => candidate.Position == "Forward"));
        Assert.Contains(refreshedSquad, candidate => candidate.Id == player.Id && candidate.Position == "Defender");

        Assert.NotNull(refreshedLineup);
        Assert.Equal(11, refreshedLineup.Lineup.StarterCount);
    }

    [Fact]
    public async Task PlayerPositionChange_AllowsSavingLineupWithThePlayerInTheNewRole()
    {
        await using var factory = new FootballManagerApiFactory();
        using var client = factory.CreateClient();

        var createdGame = await CreateArsenalGameAsync(client);
        var initialSquad = await client.GetFromJsonAsync<List<SquadPlayerDto>>(
            $"/api/squad?gameId={createdGame.GameId}");

        Assert.NotNull(initialSquad);
        var candidate = Assert.IsType<SquadPlayerDto>(
            initialSquad.FirstOrDefault(player => player.Position == "Forward" && !player.IsStarter));

        var positionChangeResponse = await client.PutAsJsonAsync(
            $"/api/player/{candidate.Id}/position?gameId={createdGame.GameId}",
            new UpdatePlayerPositionRequestDto("Defender"));
        positionChangeResponse.EnsureSuccessStatusCode();

        var updatedPlayer = await positionChangeResponse.Content.ReadFromJsonAsync<PlayerDetailDto>();
        Assert.NotNull(updatedPlayer);
        Assert.Equal("Defender", updatedPlayer.Position);

        var lineupEditor = await client.GetFromJsonAsync<LineupEditorDto>(
            $"/api/lineup?gameId={createdGame.GameId}");

        Assert.NotNull(lineupEditor);
        var formation = lineupEditor.Formations.Single(current => current.Id == lineupEditor.Lineup.FormationId);
        var lineupPlayers = lineupEditor.Players.ToList();
        var starterIds = lineupPlayers
            .Where(player => player.Position == "Goalkeeper")
            .Take(1)
            .Concat(lineupPlayers.Where(player => player.Position == "Defender" && player.Id == candidate.Id))
            .Concat(lineupPlayers.Where(player => player.Position == "Defender" && player.Id != candidate.Id).Take(formation.Defenders - 1))
            .Concat(lineupPlayers.Where(player => player.Position == "Midfielder").Take(formation.Midfielders))
            .Concat(lineupPlayers.Where(player => player.Position == "Forward").Take(formation.Forwards))
            .Select(player => player.Id)
            .ToArray();

        Assert.Equal(formation.Defenders, lineupPlayers.Count(player => starterIds.Contains(player.Id) && player.Position == "Defender"));
        Assert.Contains(candidate.Id, starterIds);

        var saveResponse = await client.PostAsJsonAsync(
            $"/api/lineup?gameId={createdGame.GameId}",
            new UpdateLineupRequestDto(lineupEditor.Lineup.FormationId, starterIds));
        saveResponse.EnsureSuccessStatusCode();

        var savedLineup = await saveResponse.Content.ReadFromJsonAsync<LineupDto>();
        var refreshedSquad = await client.GetFromJsonAsync<List<SquadPlayerDto>>(
            $"/api/squad?gameId={createdGame.GameId}");

        Assert.NotNull(savedLineup);
        Assert.Equal(11, savedLineup.StarterCount);
        Assert.Contains(savedLineup.StarterPlayerIds, playerId => playerId == candidate.Id);
        Assert.NotNull(refreshedSquad);
        Assert.Contains(refreshedSquad, player => player.Id == candidate.Id && player.Position == "Defender" && player.IsStarter);
    }

    [Fact]
    public async Task AutoLineup_UsesAvailableFallbackPlayersWhenOnePositionGroupRunsShort()
    {
        await using var factory = new FootballManagerApiFactory();
        using var client = factory.CreateClient();

        var createdGame = await CreateArsenalGameAsync(client);
        var initialSquad = await client.GetFromJsonAsync<List<SquadPlayerDto>>(
            $"/api/squad?gameId={createdGame.GameId}");

        Assert.NotNull(initialSquad);
        var defendersToMove = initialSquad
            .Where(player => player.Position == "Defender")
            .OrderByDescending(player => player.SquadNumber)
            .Take(3)
            .ToList();

        Assert.Equal(3, defendersToMove.Count);

        foreach (var defender in defendersToMove)
        {
            var response = await client.PutAsJsonAsync(
                $"/api/player/{defender.Id}/position?gameId={createdGame.GameId}",
                new UpdatePlayerPositionRequestDto("Midfielder"));
            response.EnsureSuccessStatusCode();
        }

        var dashboard = await client.GetFromJsonAsync<ClubDashboardDto>(
            $"/api/club/dashboard?gameId={createdGame.GameId}");
        var refreshedSquad = await client.GetFromJsonAsync<List<SquadPlayerDto>>(
            $"/api/squad?gameId={createdGame.GameId}");

        Assert.NotNull(dashboard);
        Assert.Equal(11, dashboard.Lineup.StarterCount);

        Assert.NotNull(refreshedSquad);
        Assert.Equal(3, refreshedSquad.Count(player => player.Position == "Defender"));
        Assert.Equal(11, refreshedSquad.Count(player => player.IsStarter));

        var simulateResponse = await client.PostAsync(
            $"/api/match/simulate-next?gameId={createdGame.GameId}",
            content: null);
        simulateResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task SimulateNextMatch_PlaysManagedFixtureAndRefreshesClubState()
    {
        await using var factory = new FootballManagerApiFactory();
        using var client = factory.CreateClient();

        var createdGame = await CreateArsenalGameAsync(client);
        var initialDashboard = await client.GetFromJsonAsync<ClubDashboardDto>(
            $"/api/club/dashboard?gameId={createdGame.GameId}");
        var initialSquad = await client.GetFromJsonAsync<List<SquadPlayerDto>>(
            $"/api/squad?gameId={createdGame.GameId}");
        var initialAcademy = await client.GetFromJsonAsync<AcademySummaryDto>(
            $"/api/academy?gameId={createdGame.GameId}");

        Assert.NotNull(initialDashboard);
        Assert.NotNull(initialSquad);
        Assert.NotNull(initialAcademy);
        var initialFixture = Assert.IsType<NextFixtureDto>(initialDashboard.NextFixture);

        var starterFitnessById = initialSquad
            .Where(player => player.IsStarter)
            .ToDictionary(player => player.Id, player => player.Fitness);

        var simulateResponse = await client.PostAsync(
            $"/api/match/simulate-next?gameId={createdGame.GameId}",
            content: null);
        simulateResponse.EnsureSuccessStatusCode();

        var simulation = await simulateResponse.Content.ReadFromJsonAsync<SimulatedMatchResultDto>();

        Assert.NotNull(simulation);
        Assert.NotEmpty(simulation.MatchEvents);
        Assert.Contains(simulation.MatchEvents, matchEvent => matchEvent.Type == "FullTime");
        Assert.NotNull(simulation.MatchMvp);
        Assert.False(string.IsNullOrWhiteSpace(simulation.MatchMvp.PlayerName));
        Assert.True(simulation.MatchMvp.MvpAwards >= 1);
        Assert.Equal(1, simulation.MatchMvp.MvpAwards);
        Assert.Equal(initialSquad.Count, simulation.SeniorPlayerDevelopment.Count);
        Assert.Equal(initialAcademy.Players.Count, simulation.AcademyDevelopment.Count);
        Assert.Contains(simulation.SeniorPlayerDevelopment, player => player.PlayedMatch);
        Assert.Contains(
            simulation.SeniorPlayerDevelopment,
            player => player.FitnessDelta != 0 || player.MoraleDelta != 0 ||
                      player.AttackDelta != 0 || player.DefenseDelta != 0 || player.PassingDelta != 0);
        Assert.Contains(
            simulation.SeniorPlayerDevelopment,
            player => player.PlayedMatch && player.Age <= 21 && player.OverallDelta > 0);
        Assert.All(simulation.AcademyDevelopment, player => Assert.True(player.DevelopmentProgressDelta >= 3));
        Assert.Equal(1, simulation.ClubStanding.Played);
        Assert.True(simulation.ClubStanding.Points is >= 0 and <= 3);
        Assert.False(string.IsNullOrWhiteSpace(simulation.Summary));

        var refreshedDashboard = await client.GetFromJsonAsync<ClubDashboardDto>(
            $"/api/club/dashboard?gameId={createdGame.GameId}");
        var refreshedSquad = await client.GetFromJsonAsync<List<SquadPlayerDto>>(
            $"/api/squad?gameId={createdGame.GameId}");

        Assert.NotNull(refreshedDashboard);
        var refreshedResult = Assert.IsType<RecentResultDto>(refreshedDashboard.RecentResult);
        var refreshedNextFixture = Assert.IsType<NextFixtureDto>(refreshedDashboard.NextFixture);
        Assert.True(refreshedNextFixture.RoundNumber > initialFixture.RoundNumber);
        Assert.Equal(simulation.ClubStanding.Position, refreshedDashboard.LeaguePosition);
        Assert.Equal(simulation.ClubStanding.Points, refreshedDashboard.Points);
        Assert.Equal(simulation.Score.HomeGoals, refreshedResult.HomeGoals);
        Assert.Equal(simulation.Score.AwayGoals, refreshedResult.AwayGoals);

        Assert.NotNull(refreshedSquad);
        var refreshedSquadById = refreshedSquad.ToDictionary(player => player.Id);
        Assert.All(simulation.SeniorPlayerDevelopment, change =>
        {
            var squadPlayer = refreshedSquadById[change.PlayerId];
            Assert.Equal(change.Attack, squadPlayer.Attack);
            Assert.Equal(change.Defense, squadPlayer.Defense);
            Assert.Equal(change.Passing, squadPlayer.Passing);
            Assert.Equal(change.Fitness, squadPlayer.Fitness);
            Assert.Equal(change.Morale, squadPlayer.Morale);
            Assert.Equal(change.OverallRating, squadPlayer.OverallRating);
        });
        Assert.Contains(
            refreshedSquad,
            player => starterFitnessById.TryGetValue(player.Id, out var initialFitness) && player.Fitness < initialFitness);
    }

    [Fact]
    public async Task CreateNewGame_AlwaysStartsSeasonOne()
    {
        await using var factory = new FootballManagerApiFactory();
        using var client = factory.CreateClient();

        var firstGame = await CreateArsenalGameAsync(client);
        var secondGame = await CreateArsenalGameAsync(client);

        Assert.Equal("Season 1", firstGame.SeasonName);
        Assert.Equal("Season 1", secondGame.SeasonName);
    }

    [Fact]
    public async Task SaveAndLoadEndpoints_ReturnRecentSavesAndSelectedCareer()
    {
        await using var factory = new FootballManagerApiFactory();
        using var client = factory.CreateClient();

        var firstGame = await CreateArsenalGameAsync(client);
        var clubs = await client.GetFromJsonAsync<List<ClubOptionDto>>("/api/game/clubs");
        var secondClub = Assert.IsType<ClubOptionDto>(clubs!.FirstOrDefault(club => club.Name == "Northbridge FC"));

        var secondCreateResponse = await client.PostAsJsonAsync("/api/game/new", new CreateNewGameRequestDto(secondClub.Id));
        secondCreateResponse.EnsureSuccessStatusCode();
        var secondGame = await secondCreateResponse.Content.ReadFromJsonAsync<CreateNewGameResponseDto>();
        Assert.NotNull(secondGame);

        var firstSaveResponse = await client.PostAsJsonAsync(
            $"/api/game/save?gameId={firstGame.GameId}",
            new SaveGameRequestDto("Arsenal Return"));
        firstSaveResponse.EnsureSuccessStatusCode();

        var secondSaveResponse = await client.PostAsJsonAsync(
            $"/api/game/save?gameId={secondGame.GameId}",
            new SaveGameRequestDto("Northbridge Return"));
        secondSaveResponse.EnsureSuccessStatusCode();

        var saveLibrary = await client.GetFromJsonAsync<LoadGameResponseDto>("/api/game/load");
        Assert.NotNull(saveLibrary);
        Assert.True(saveLibrary.Saves.Count >= 2);
        Assert.Equal("Northbridge Return", saveLibrary.Saves.First().SaveName);
        Assert.Contains(saveLibrary.Saves, save => save.SaveName == "Arsenal Return");

        var selectedSaveResponse = await client.GetFromJsonAsync<LoadGameResponseDto>(
            $"/api/game/load?gameId={firstGame.GameId}");

        Assert.NotNull(selectedSaveResponse);
        Assert.NotNull(selectedSaveResponse.SelectedSave);
        Assert.Equal(firstGame.GameId, selectedSaveResponse.SelectedSave.GameId);
        Assert.Equal("Arsenal Return", selectedSaveResponse.SelectedSave.SaveName);

        var deleteResponse = await client.DeleteAsync($"/api/game/save?gameId={firstGame.GameId}");
        deleteResponse.EnsureSuccessStatusCode();

        var deletedSave = await deleteResponse.Content.ReadFromJsonAsync<GameSaveSummaryDto>();
        Assert.NotNull(deletedSave);
        Assert.Equal(firstGame.GameId, deletedSave.GameId);
        Assert.Equal("Arsenal Return", deletedSave.SaveName);

        var refreshedLibrary = await client.GetFromJsonAsync<LoadGameResponseDto>("/api/game/load");
        Assert.NotNull(refreshedLibrary);
        Assert.DoesNotContain(refreshedLibrary.Saves, save => save.GameId == firstGame.GameId);
        Assert.Contains(refreshedLibrary.Saves, save => save.GameId == secondGame.GameId);

        var missingSaveResponse = await client.GetAsync($"/api/game/load?gameId={firstGame.GameId}");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, missingSaveResponse.StatusCode);
    }

    [Fact]
    public async Task LeagueTableAndFixturesEndpoints_ReturnSeasonProgressAfterSimulation()
    {
        await using var factory = new FootballManagerApiFactory();
        using var client = factory.CreateClient();

        var createdGame = await CreateArsenalGameAsync(client);

        var initialTable = await client.GetFromJsonAsync<List<LeagueTableEntryDto>>(
            $"/api/league/table?gameId={createdGame.GameId}");
        var initialFixtures = await client.GetFromJsonAsync<List<FixtureSummaryDto>>(
            $"/api/fixtures?gameId={createdGame.GameId}");

        Assert.NotNull(initialTable);
        Assert.Equal(8, initialTable.Count);
        Assert.NotNull(initialFixtures);
        Assert.Equal(56, initialFixtures.Count);
        Assert.DoesNotContain(initialFixtures, fixture => fixture.IsPlayed);
        Assert.Equal(4, initialFixtures.Count(fixture => fixture.IsCurrentRound));

        var simulateResponse = await client.PostAsync(
            $"/api/match/simulate-next?gameId={createdGame.GameId}",
            content: null);
        simulateResponse.EnsureSuccessStatusCode();

        var refreshedTable = await client.GetFromJsonAsync<List<LeagueTableEntryDto>>(
            $"/api/league/table?gameId={createdGame.GameId}");
        var refreshedFixtures = await client.GetFromJsonAsync<List<FixtureSummaryDto>>(
            $"/api/fixtures?gameId={createdGame.GameId}");

        Assert.NotNull(refreshedTable);
        var arsenalRow = Assert.Single(refreshedTable, entry => entry.ClubName == "Arsenal");
        Assert.Equal(1, arsenalRow.Played);

        Assert.NotNull(refreshedFixtures);
        Assert.Equal(4, refreshedFixtures.Count(fixture => fixture.IsPlayed));
        Assert.Equal(4, refreshedFixtures.Count(fixture => fixture.IsCurrentRound));
        Assert.Contains(refreshedFixtures, fixture => fixture.IsManagedClubFixture && fixture.IsPlayed);
    }

    [Fact]
    public async Task StartNextSeason_AfterSeasonEnds_GeneratesFreshFixturesForTheSave()
    {
        await using var factory = new FootballManagerApiFactory();
        using var client = factory.CreateClient();

        var createdGame = await CreateArsenalGameAsync(client);
        var initialFixtures = await client.GetFromJsonAsync<List<FixtureSummaryDto>>(
            $"/api/fixtures?gameId={createdGame.GameId}");
        var dashboard = await client.GetFromJsonAsync<ClubDashboardDto>(
            $"/api/club/dashboard?gameId={createdGame.GameId}");

        Assert.NotNull(initialFixtures);
        Assert.NotNull(dashboard);

        for (var round = 0; round < 20 && dashboard.NextFixture is not null; round++)
        {
            var simulateResponse = await client.PostAsync(
                $"/api/match/simulate-next?gameId={createdGame.GameId}",
                content: null);
            Assert.True(
                simulateResponse.IsSuccessStatusCode,
                await simulateResponse.Content.ReadAsStringAsync());

            dashboard = await client.GetFromJsonAsync<ClubDashboardDto>(
                $"/api/club/dashboard?gameId={createdGame.GameId}");
            Assert.NotNull(dashboard);
        }

        var finishedDashboard = dashboard;

        Assert.Null(finishedDashboard.NextFixture);
        Assert.Equal("Season 1", finishedDashboard.SeasonName);

        var startSeasonResponse = await client.PostAsync(
            $"/api/match/start-next-season?gameId={createdGame.GameId}",
            content: null);
        startSeasonResponse.EnsureSuccessStatusCode();

        var startSeason = await startSeasonResponse.Content.ReadFromJsonAsync<StartNextSeasonResultDto>();
        var refreshedDashboard = await client.GetFromJsonAsync<ClubDashboardDto>(
            $"/api/club/dashboard?gameId={createdGame.GameId}");
        var refreshedFixtures = await client.GetFromJsonAsync<List<FixtureSummaryDto>>(
            $"/api/fixtures?gameId={createdGame.GameId}");
        var saveLibrary = await client.GetFromJsonAsync<LoadGameResponseDto>(
            $"/api/game/load?gameId={createdGame.GameId}");

        Assert.NotNull(startSeason);
        Assert.Equal("Season 2", startSeason.SeasonName);
        Assert.NotNull(startSeason.NextFixture);
        Assert.False(string.IsNullOrWhiteSpace(startSeason.Summary));

        Assert.NotNull(refreshedDashboard);
        Assert.Equal("Season 2", refreshedDashboard.SeasonName);
        Assert.NotNull(refreshedDashboard.NextFixture);
        Assert.Equal(0, refreshedDashboard.Points);

        Assert.NotNull(refreshedFixtures);
        Assert.Equal(initialFixtures.Count, refreshedFixtures.Count);
        Assert.DoesNotContain(refreshedFixtures, fixture => fixture.IsPlayed);
        Assert.Equal(4, refreshedFixtures.Count(fixture => fixture.IsCurrentRound));

        Assert.NotNull(saveLibrary);
        Assert.NotNull(saveLibrary.SelectedSave);
        Assert.Equal("Season 2", saveLibrary.SelectedSave.SeasonName);
    }

    [Fact]
    public async Task TopPlayersEndpoint_ReturnsCurrentSeasonMvpLeaderboard()
    {
        await using var factory = new FootballManagerApiFactory();
        using var client = factory.CreateClient();

        var createdGame = await CreateArsenalGameAsync(client);

        var simulateResponse = await client.PostAsync(
            $"/api/match/simulate-next?gameId={createdGame.GameId}",
            content: null);
        simulateResponse.EnsureSuccessStatusCode();

        var simulation = await simulateResponse.Content.ReadFromJsonAsync<SimulatedMatchResultDto>();
        var topPlayers = await client.GetFromJsonAsync<List<TopPlayerDto>>(
            $"/api/league/top-players?gameId={createdGame.GameId}");

        Assert.NotNull(simulation);
        Assert.NotNull(topPlayers);
        Assert.NotEmpty(topPlayers);
        Assert.True(topPlayers.Count <= 10);
        Assert.Equal(4, topPlayers.Sum(player => player.MvpAwards));
        Assert.Contains(topPlayers, player => player.PlayerId == simulation.MatchMvp.PlayerId);
        Assert.All(topPlayers, player =>
        {
            Assert.False(string.IsNullOrWhiteSpace(player.PlayerName));
            Assert.False(string.IsNullOrWhiteSpace(player.ClubName));
            Assert.True(player.MvpAwards > 0);
        });

        for (var index = 1; index < topPlayers.Count; index++)
        {
            Assert.True(topPlayers[index - 1].MvpAwards >= topPlayers[index].MvpAwards);
        }
    }

    [Fact]
    public async Task TransferMarketEndpoints_BuyAndSellPlayersAndUpdateBudgetAndSquad()
    {
        await using var factory = new FootballManagerApiFactory();
        using var client = factory.CreateClient();

        var createdGame = await CreateArsenalGameAsync(client);

        var initialMarket = await client.GetFromJsonAsync<TransferMarketDto>(
            $"/api/transfer/market?gameId={createdGame.GameId}");

        Assert.NotNull(initialMarket);
        Assert.NotEmpty(initialMarket.Targets);
        var target = Assert.IsType<TransferMarketPlayerDto>(initialMarket.Targets.FirstOrDefault(player => player.IsAffordable));

        var buyResponse = await client.PostAsJsonAsync(
            $"/api/transfer/buy?gameId={createdGame.GameId}",
            new BuyTransferRequestDto(target.PlayerId));
        buyResponse.EnsureSuccessStatusCode();

        var buyResult = await buyResponse.Content.ReadFromJsonAsync<TransferActionResultDto>();
        Assert.NotNull(buyResult);
        Assert.True(buyResult.AvailableBudget < initialMarket.AvailableBudget);

        var squadAfterBuy = await client.GetFromJsonAsync<List<SquadPlayerDto>>(
            $"/api/squad?gameId={createdGame.GameId}");
        Assert.NotNull(squadAfterBuy);
        Assert.Contains(squadAfterBuy, player => player.Id == target.PlayerId);
        Assert.Single(squadAfterBuy, player => player.IsCaptain);

        var marketAfterBuy = await client.GetFromJsonAsync<TransferMarketDto>(
            $"/api/transfer/market?gameId={createdGame.GameId}");
        Assert.NotNull(marketAfterBuy);
        Assert.Contains(marketAfterBuy.RecentActivity, activity => activity.PlayerName == target.Name && activity.IsIncoming);

        var sale = Assert.IsType<SaleOpportunityDto>(
            marketAfterBuy.SaleOpportunities.FirstOrDefault(player => player.SuggestedBuyer != "No buyer lined up"));

        var sellResponse = await client.PostAsJsonAsync(
            $"/api/transfer/sell?gameId={createdGame.GameId}",
            new SellTransferRequestDto(sale.PlayerId));
        sellResponse.EnsureSuccessStatusCode();

        var sellResult = await sellResponse.Content.ReadFromJsonAsync<TransferActionResultDto>();
        Assert.NotNull(sellResult);
        Assert.True(sellResult.AvailableBudget > buyResult.AvailableBudget);

        var squadAfterSell = await client.GetFromJsonAsync<List<SquadPlayerDto>>(
            $"/api/squad?gameId={createdGame.GameId}");
        Assert.NotNull(squadAfterSell);
        Assert.DoesNotContain(squadAfterSell, player => player.Id == sale.PlayerId);
        Assert.Single(squadAfterSell, player => player.IsCaptain);
    }

    [Fact]
    public async Task FinanceEndpoint_TracksWagesTransferSpendingAndMatchIncome()
    {
        await using var factory = new FootballManagerApiFactory();
        using var client = factory.CreateClient();

        var createdGame = await CreateArsenalGameAsync(client);

        var initialFinance = await client.GetFromJsonAsync<FinanceSummaryDto>(
            $"/api/finance?gameId={createdGame.GameId}");

        Assert.NotNull(initialFinance);
        Assert.Equal("Arsenal", initialFinance.ClubName);
        Assert.True(initialFinance.WageTotal > 0);
        Assert.Equal(0, initialFinance.TransferSpending);
        Assert.Equal(0, initialFinance.TransferIncome);
        Assert.Equal(0, initialFinance.MatchIncome);

        var fixtures = await client.GetFromJsonAsync<List<FixtureSummaryDto>>(
            $"/api/fixtures?gameId={createdGame.GameId}");
        Assert.NotNull(fixtures);

        var firstHomeRound = fixtures
            .Where(fixture => fixture.IsManagedClubFixture && fixture.HomeClub == "Arsenal")
            .Min(fixture => fixture.RoundNumber);

        for (var round = 0; round < firstHomeRound; round++)
        {
            var simulateResponse = await client.PostAsync(
                $"/api/match/simulate-next?gameId={createdGame.GameId}",
                content: null);
            simulateResponse.EnsureSuccessStatusCode();
        }

        var market = await client.GetFromJsonAsync<TransferMarketDto>(
            $"/api/transfer/market?gameId={createdGame.GameId}");
        Assert.NotNull(market);

        var target = Assert.IsType<TransferMarketPlayerDto>(
            market.Targets.FirstOrDefault(player => player.IsAffordable));
        var buyResponse = await client.PostAsJsonAsync(
            $"/api/transfer/buy?gameId={createdGame.GameId}",
            new BuyTransferRequestDto(target.PlayerId));
        buyResponse.EnsureSuccessStatusCode();

        var marketAfterBuy = await client.GetFromJsonAsync<TransferMarketDto>(
            $"/api/transfer/market?gameId={createdGame.GameId}");
        Assert.NotNull(marketAfterBuy);

        var sale = Assert.IsType<SaleOpportunityDto>(
            marketAfterBuy.SaleOpportunities.FirstOrDefault(player => player.SuggestedBuyer != "No buyer lined up"));
        var sellResponse = await client.PostAsJsonAsync(
            $"/api/transfer/sell?gameId={createdGame.GameId}",
            new SellTransferRequestDto(sale.PlayerId));
        sellResponse.EnsureSuccessStatusCode();

        var finance = await client.GetFromJsonAsync<FinanceSummaryDto>(
            $"/api/finance?gameId={createdGame.GameId}");

        Assert.NotNull(finance);
        Assert.True(finance.MatchIncome > 0);
        Assert.True(finance.TransferSpending > 0);
        Assert.True(finance.TransferIncome > 0);
        Assert.NotEqual(initialFinance.CurrentBudget, finance.CurrentBudget);
        Assert.NotEmpty(finance.RecentEvents);
        Assert.Contains(finance.RecentEvents, entry => entry.Type == "Wages");
        Assert.Contains(finance.RecentEvents, entry => entry.Type == "Match income");
        Assert.Contains(finance.RecentEvents, entry => entry.Type == "Transfer spend");
        Assert.Contains(finance.RecentEvents, entry => entry.Type == "Transfer income");
    }

    [Fact]
    public async Task AcademyEndpoints_ShowYouthProspectsDevelopAndAllowPromotion()
    {
        await using var factory = new FootballManagerApiFactory();
        using var client = factory.CreateClient();

        var createdGame = await CreateArsenalGameAsync(client);

        var initialAcademy = await client.GetFromJsonAsync<AcademySummaryDto>(
            $"/api/academy?gameId={createdGame.GameId}");
        var initialSquad = await client.GetFromJsonAsync<List<SquadPlayerDto>>(
            $"/api/squad?gameId={createdGame.GameId}");

        Assert.NotNull(initialAcademy);
        Assert.NotNull(initialSquad);
        Assert.Equal("Arsenal", initialAcademy.ClubName);
        Assert.Equal(5, initialAcademy.Players.Count);
        Assert.NotNull(initialAcademy.SpotlightPlayer);
        Assert.Contains(initialAcademy.Players, player => player.Name == "David Seaman");
        Assert.Contains(initialAcademy.Players, player => player.Name == "Patrick Vieira");
        Assert.Contains(initialAcademy.Players, player => player.Name == "Thierry Henry");
        Assert.DoesNotContain(initialAcademy.Players, player => player.Name == "Julian Reed");
        Assert.DoesNotContain(initialAcademy.Players, player => player.Name == "Isaac Pereira");
        Assert.DoesNotContain(initialAcademy.Players, player => player.Name == "Victor Foster");

        var trackedProspect = initialAcademy.Players.First();
        var initialDevelopment = trackedProspect.DevelopmentProgress;

        var simulateResponse = await client.PostAsync(
            $"/api/match/simulate-next?gameId={createdGame.GameId}",
            content: null);
        simulateResponse.EnsureSuccessStatusCode();
        var simulation = await simulateResponse.Content.ReadFromJsonAsync<SimulatedMatchResultDto>();

        Assert.NotNull(simulation);
        Assert.Equal(initialAcademy.Players.Count, simulation.AcademyDevelopment.Count);
        Assert.All(simulation.AcademyDevelopment, player => Assert.True(player.DevelopmentProgressDelta >= 3));
        Assert.Contains(
            simulation.AcademyDevelopment,
            player => player.PlayerId == trackedProspect.PlayerId && player.DevelopmentProgressDelta > 0);

        var academyAfterMatch = await client.GetFromJsonAsync<AcademySummaryDto>(
            $"/api/academy?gameId={createdGame.GameId}");

        Assert.NotNull(academyAfterMatch);
        var progressedProspect = Assert.Single(
            academyAfterMatch.Players,
            player => player.PlayerId == trackedProspect.PlayerId);
        Assert.True(progressedProspect.DevelopmentProgress >= initialDevelopment);
        var academyPlayersById = academyAfterMatch.Players.ToDictionary(player => player.PlayerId);
        Assert.All(simulation.AcademyDevelopment, change =>
        {
            var academyPlayer = academyPlayersById[change.PlayerId];
            Assert.Equal(change.OverallRating, academyPlayer.OverallRating);
            Assert.Equal(change.DevelopmentProgress, academyPlayer.DevelopmentProgress);
        });

        var promotionCandidate = academyAfterMatch.Players
            .OrderByDescending(player => player.IsPromotionReady)
            .ThenByDescending(player => player.PromotionReadiness)
            .First();

        var promoteResponse = await client.PostAsJsonAsync(
            $"/api/academy/promote?gameId={createdGame.GameId}",
            new PromoteAcademyPlayerRequestDto(promotionCandidate.PlayerId));
        promoteResponse.EnsureSuccessStatusCode();

        var promotion = await promoteResponse.Content.ReadFromJsonAsync<AcademyPromotionResultDto>();
        var academyAfterPromotion = await client.GetFromJsonAsync<AcademySummaryDto>(
            $"/api/academy?gameId={createdGame.GameId}");
        var squadAfterPromotion = await client.GetFromJsonAsync<List<SquadPlayerDto>>(
            $"/api/squad?gameId={createdGame.GameId}");

        Assert.NotNull(promotion);
        Assert.Equal(promotionCandidate.PlayerId, promotion.AcademyPlayerId);
        Assert.NotNull(academyAfterPromotion);
        Assert.Equal(4, academyAfterPromotion.Players.Count);
        Assert.DoesNotContain(academyAfterPromotion.Players, player => player.PlayerId == promotionCandidate.PlayerId);
        Assert.NotNull(squadAfterPromotion);
        Assert.Equal(initialSquad.Count + 1, squadAfterPromotion.Count);
        Assert.Contains(squadAfterPromotion, player => player.Id == promotion.SeniorPlayerId && player.Name == promotion.PlayerName);
    }

    [Fact]
    public async Task MatchSimulation_AppliesAgeBasedAttributeChangesAcrossRounds()
    {
        await using var factory = new FootballManagerApiFactory();
        using var client = factory.CreateClient();

        var createdGame = await CreateArsenalGameAsync(client);
        var initialSquad = await client.GetFromJsonAsync<List<SquadPlayerDto>>(
            $"/api/squad?gameId={createdGame.GameId}");

        Assert.NotNull(initialSquad);
        var ageCurveCoreRatings = initialSquad
            .Where(player => player.Age <= 24 || player.Age >= 30)
            .ToDictionary(
                player => player.Id,
                player => player.Attack + player.Defense + player.Passing);
        Assert.NotEmpty(ageCurveCoreRatings);

        for (var round = 0; round < 4; round++)
        {
            var simulateResponse = await client.PostAsync(
                $"/api/match/simulate-next?gameId={createdGame.GameId}",
                content: null);
            simulateResponse.EnsureSuccessStatusCode();
        }

        var refreshedSquad = await client.GetFromJsonAsync<List<SquadPlayerDto>>(
            $"/api/squad?gameId={createdGame.GameId}");

        Assert.NotNull(refreshedSquad);
        Assert.Contains(
            refreshedSquad,
            player => ageCurveCoreRatings.TryGetValue(player.Id, out var initialValue) &&
                      (player.Attack + player.Defense + player.Passing) != initialValue);
    }

    private static async Task<CreateNewGameResponseDto> CreateArsenalGameAsync(HttpClient client)
    {
        var clubs = await client.GetFromJsonAsync<List<ClubOptionDto>>("/api/game/clubs");
        var selectedClub = Assert.Single(clubs!, club => club.Name == "Arsenal");

        var createResponse = await client.PostAsJsonAsync("/api/game/new", new CreateNewGameRequestDto(selectedClub.Id));
        createResponse.EnsureSuccessStatusCode();

        var createdGame = await createResponse.Content.ReadFromJsonAsync<CreateNewGameResponseDto>();
        return Assert.IsType<CreateNewGameResponseDto>(createdGame);
    }
}
