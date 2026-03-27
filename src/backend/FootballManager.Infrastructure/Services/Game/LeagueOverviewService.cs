using FootballManager.Application.Contracts;
using FootballManager.Application.Services;
using FootballManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FootballManager.Infrastructure.Services.Game;

public sealed class LeagueOverviewService(FootballManagerDbContext dbContext) : ILeagueOverviewService
{
    public async Task<IReadOnlyCollection<LeagueTableEntryDto>?> GetLeagueTableAsync(
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        var gameSave = await LoadGameSaveAsync(gameId, cancellationToken);
        if (gameSave?.SelectedClub?.League is null || gameSave.Season is null)
        {
            return null;
        }

        return LeagueTableCalculator.BuildTable(
            gameSave.SelectedClub.League.Clubs.ToList(),
            gameSave.Season.Fixtures.ToList());
    }

    public async Task<IReadOnlyCollection<FixtureSummaryDto>?> GetFixturesAsync(
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        var gameSave = await LoadGameSaveAsync(gameId, cancellationToken);
        if (gameSave?.SelectedClub?.League is null || gameSave.Season is null)
        {
            return null;
        }

        var clubNames = gameSave.SelectedClub.League.Clubs
            .ToDictionary(club => club.Id, club => club.Name);
        var currentRound = gameSave.Season.CurrentRound;

        return gameSave.Season.Fixtures
            .OrderBy(fixture => fixture.RoundNumber)
            .ThenBy(fixture => fixture.ScheduledAt)
            .Select(fixture => new FixtureSummaryDto(
                fixture.Id,
                clubNames[fixture.HomeClubId],
                clubNames[fixture.AwayClubId],
                fixture.RoundNumber,
                fixture.ScheduledAt,
                fixture.IsPlayed,
                fixture.HomeGoals,
                fixture.AwayGoals,
                fixture.PlayedAt,
                fixture.HomeClubId == gameSave.SelectedClubId || fixture.AwayClubId == gameSave.SelectedClubId,
                !fixture.IsPlayed && fixture.RoundNumber == currentRound))
            .ToList();
    }

    public async Task<IReadOnlyCollection<TopPlayerDto>?> GetTopPlayersAsync(
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        var gameSave = await dbContext.GameSaves
            .Include(save => save.SelectedClub)
            .Include(save => save.Season)
            .SingleOrDefaultAsync(save => save.Id == gameId, cancellationToken);

        if (gameSave?.SelectedClub is null || gameSave.Season is null)
        {
            return null;
        }

        var awards = await dbContext.Fixtures
            .Where(fixture => fixture.SeasonId == gameSave.Season.Id &&
                              fixture.IsPlayed &&
                              fixture.MatchMvpPlayerId.HasValue)
            .GroupBy(fixture => fixture.MatchMvpPlayerId!.Value)
            .Select(group => new
            {
                PlayerId = group.Key,
                MvpAwards = group.Count()
            })
            .ToListAsync(cancellationToken);

        if (awards.Count == 0)
        {
            return [];
        }

        var playerIds = awards.Select(award => award.PlayerId).ToHashSet();
        var players = await dbContext.Players
            .Include(player => player.Club)
            .Where(player => playerIds.Contains(player.Id))
            .ToDictionaryAsync(player => player.Id, cancellationToken);

        return awards
            .Where(award => players.ContainsKey(award.PlayerId))
            .Select(award =>
            {
                var player = players[award.PlayerId];

                return new TopPlayerDto(
                    player.Id,
                    player.FullName,
                    player.Club?.Name ?? "No Club",
                    player.Position.ToString(),
                    player.SquadNumber,
                    player.GetOverallRating(),
                    award.MvpAwards);
            })
            .OrderByDescending(player => player.MvpAwards)
            .ThenByDescending(player => player.OverallRating)
            .ThenBy(player => player.PlayerName)
            .Take(10)
            .ToList();
    }

    private async Task<FootballManager.Domain.Entities.GameSave?> LoadGameSaveAsync(Guid gameId, CancellationToken cancellationToken)
    {
        return await dbContext.GameSaves
            .Include(save => save.SelectedClub)
                .ThenInclude(club => club!.League)
                    .ThenInclude(league => league!.Clubs)
            .Include(save => save.Season)
                .ThenInclude(season => season!.Fixtures)
            .SingleOrDefaultAsync(save => save.Id == gameId, cancellationToken);
    }
}
