using FootballManager.Application.Contracts;
using FootballManager.Application.Services;
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
        var standings = leagueClubs
            .OrderBy(club => club.Name)
            .Select((club, index) => new { club.Id, Position = index + 1 })
            .ToDictionary(item => item.Id, item => item.Position);

        var nextFixture = gameSave.Season.Fixtures
            .Where(fixture => !fixture.IsPlayed &&
                              (fixture.HomeClubId == selectedClub.Id || fixture.AwayClubId == selectedClub.Id))
            .OrderBy(fixture => fixture.RoundNumber)
            .ThenBy(fixture => fixture.ScheduledAt)
            .FirstOrDefault();

        var clubNames = leagueClubs.ToDictionary(club => club.Id, club => club.Name);
        var squadSummary = SquadViewFactory.BuildSquadSummary(selectedClub.Players.ToList(), starterIds);
        var lineupSummary = SquadViewFactory.BuildLineup(lineup, selectedClub.Players.ToList());
        var featuredPlayer = SquadViewFactory.BuildFeaturedPlayer(selectedClub.Players.ToList(), starterIds);

        return new ClubDashboardDto(
            selectedClub.Name,
            gameSave.Season.Name,
            selectedClub.TransferBudget,
            standings[selectedClub.Id],
            0,
            nextFixture is null
                ? null
                : new NextFixtureDto(
                    clubNames[nextFixture.HomeClubId],
                    clubNames[nextFixture.AwayClubId],
                    nextFixture.ScheduledAt,
                    nextFixture.RoundNumber),
            squadSummary,
            lineupSummary,
            featuredPlayer);
    }
}
