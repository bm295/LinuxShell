using FootballManager.Application.Contracts;
using FootballManager.Application.Services;
using FootballManager.Domain.Entities;
using FootballManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FootballManager.Infrastructure.Services.Game;

public sealed class ClubDashboardService(FootballManagerDbContext dbContext) : IClubDashboardService
{
    public async Task<ClubDashboardDto?> GetDashboardAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        var gameSave = await dbContext.GameSaves
            .Include(save => save.SelectedClub)
                .ThenInclude(club => club!.Players)
            .Include(save => save.Lineup)
                .ThenInclude(lineup => lineup!.Formation)
            .Include(save => save.SelectedClub)
                .ThenInclude(club => club!.League)
                    .ThenInclude(league => league!.Clubs)
            .Include(save => save.Season)
                .ThenInclude(season => season!.Fixtures)
            .SingleOrDefaultAsync(save => save.Id == gameId, cancellationToken);

        if (gameSave?.SelectedClub is null || gameSave.Season is null)
        {
            return null;
        }

        var selectedClub = gameSave.SelectedClub;
        var lineup = await LineupPlanner.EnsureLineupAsync(dbContext, gameSave, cancellationToken);
        var starterIds = lineup.GetStarterPlayerIds().ToHashSet();
        var leagueClubs = selectedClub.League?.Clubs.OrderBy(club => club.Name).ToList() ?? [selectedClub];
        var leagueTable = LeagueTableCalculator.BuildTable(leagueClubs, gameSave.Season.Fixtures.ToList());
        var clubStanding = leagueTable.Single(entry => entry.ClubId == selectedClub.Id);

        var nextFixture = gameSave.Season.Fixtures
            .Where(fixture => !fixture.IsPlayed &&
                              (fixture.HomeClubId == selectedClub.Id || fixture.AwayClubId == selectedClub.Id))
            .OrderBy(fixture => fixture.RoundNumber)
            .ThenBy(fixture => fixture.ScheduledAt)
            .FirstOrDefault();

        var recentResult = gameSave.Season.Fixtures
            .Where(fixture => fixture.IsPlayed &&
                              (fixture.HomeClubId == selectedClub.Id || fixture.AwayClubId == selectedClub.Id))
            .OrderByDescending(fixture => fixture.PlayedAt ?? fixture.ScheduledAt)
            .ThenByDescending(fixture => fixture.RoundNumber)
            .FirstOrDefault();

        var clubNames = leagueClubs.ToDictionary(club => club.Id, club => club.Name);
        var squadSummary = SquadViewFactory.BuildSquadSummary(selectedClub.Players.ToList(), starterIds);
        var lineupSummary = SquadViewFactory.BuildLineup(lineup, selectedClub.Players.ToList());
        var featuredPlayer = SquadViewFactory.BuildFeaturedPlayer(selectedClub.Players.ToList(), starterIds);

        return new ClubDashboardDto(
            selectedClub.Name,
            gameSave.Season.Name,
            selectedClub.League?.Name ?? "League Play",
            selectedClub.TransferBudget,
            clubStanding.Position,
            clubStanding.Points,
            MapNextFixture(nextFixture, clubNames),
            MapRecentResult(recentResult, clubNames),
            BuildMomentumNote(selectedClub, recentResult, lineupSummary),
            squadSummary,
            lineupSummary,
            featuredPlayer);
    }

    private static NextFixtureDto? MapNextFixture(Fixture? fixture, IReadOnlyDictionary<Guid, string> clubNames) =>
        fixture is null
            ? null
            : new NextFixtureDto(
                clubNames[fixture.HomeClubId],
                clubNames[fixture.AwayClubId],
                fixture.ScheduledAt,
                fixture.RoundNumber);

    private static RecentResultDto? MapRecentResult(Fixture? fixture, IReadOnlyDictionary<Guid, string> clubNames)
    {
        if (fixture?.HomeGoals is null || fixture.AwayGoals is null)
        {
            return null;
        }

        return new RecentResultDto(
            clubNames[fixture.HomeClubId],
            clubNames[fixture.AwayClubId],
            fixture.HomeGoals.Value,
            fixture.AwayGoals.Value,
            fixture.PlayedAt ?? fixture.ScheduledAt,
            fixture.RoundNumber);
    }

    private static string BuildMomentumNote(Club selectedClub, Fixture? recentResult, LineupDto lineup)
    {
        var averageMorale = selectedClub.Players.Count == 0
            ? 0
            : (int)Math.Round(selectedClub.Players.Average(player => player.Morale), MidpointRounding.AwayFromZero);
        var averageFitness = selectedClub.Players.Count == 0
            ? 0
            : (int)Math.Round(selectedClub.Players.Average(player => player.Fitness), MidpointRounding.AwayFromZero);

        if (recentResult?.HomeGoals is null || recentResult.AwayGoals is null)
        {
            return lineup.Readiness switch
            {
                "Match sharp" => "The camp feels primed for kickoff. The next whistle is all anyone is talking about.",
                "Ready for kickoff" => "The build-up feels steady. One more clear selection call and the side is set.",
                _ => "The squad still need a sharper edge before the next fixture turns live."
            };
        }

        var selectedClubWon = recentResult.HomeClubId == selectedClub.Id
            ? recentResult.HomeGoals.Value > recentResult.AwayGoals.Value
            : recentResult.AwayGoals.Value > recentResult.HomeGoals.Value;
        var selectedClubDrew = recentResult.HomeGoals.Value == recentResult.AwayGoals.Value;

        if (selectedClubWon && averageMorale >= 78)
        {
            return "Belief is surging after the last result. The dressing room wants the next match right now.";
        }

        if (selectedClubWon)
        {
            return "The last result put a lift through the squad. Keep the tempo and this can become a run.";
        }

        if (selectedClubDrew && averageFitness < 74)
        {
            return "The last outing kept the table moving, but the legs look heavy going into the next round.";
        }

        if (!selectedClubDrew && averageMorale < 74)
        {
            return "The room took the last result hard. The next team talk has to reset the edge.";
        }

        if (averageFitness < 72)
        {
            return "Heavy legs are starting to shape the next selection. Rotation will matter before kickoff.";
        }

        return "The next fixture is close enough to feel. Form is still in the balance.";
    }
}
