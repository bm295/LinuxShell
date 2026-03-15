using FootballManager.Application.Contracts;
using FootballManager.Application.Services;
using FootballManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FootballManager.Infrastructure.Services.Game;

public sealed class GameSaveService(FootballManagerDbContext dbContext) : IGameSaveService
{
    public async Task<GameSaveSummaryDto?> SaveAsync(
        Guid gameId,
        SaveGameRequestDto request,
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

        gameSave.Save(request.SaveName);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapSummary(gameSave.Id, gameSave.SaveName, gameSave.SelectedClub.Name, gameSave.Season.Name, gameSave.CreatedAt, gameSave.LastSavedAt);
    }

    public async Task<LoadGameResponseDto> LoadAsync(Guid? gameId, CancellationToken cancellationToken = default)
    {
        var saves = await dbContext.GameSaves
            .AsNoTracking()
            .OrderByDescending(save => save.LastSavedAt)
            .ThenByDescending(save => save.CreatedAt)
            .Select(save => new GameSaveSummaryDto(
                save.Id,
                save.SaveName,
                save.SelectedClub!.Name,
                save.Season!.Name,
                save.CreatedAt,
                save.LastSavedAt))
            .ToListAsync(cancellationToken);

        var selectedSave = gameId is null
            ? null
            : saves.SingleOrDefault(save => save.GameId == gameId.Value);

        return new LoadGameResponseDto(selectedSave, saves);
    }

    public async Task<GameSaveSummaryDto?> DeleteAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        var gameSave = await dbContext.GameSaves
            .Include(save => save.SelectedClub)
            .Include(save => save.Season)
            .SingleOrDefaultAsync(save => save.Id == gameId, cancellationToken);

        if (gameSave?.SelectedClub is null || gameSave.Season is null)
        {
            return null;
        }

        var leagueId = gameSave.Season.LeagueId;
        var deletedSave = MapSummary(
            gameSave.Id,
            gameSave.SaveName,
            gameSave.SelectedClub.Name,
            gameSave.Season.Name,
            gameSave.CreatedAt,
            gameSave.LastSavedAt);

        var clubIds = await dbContext.Clubs
            .Where(club => club.LeagueId == leagueId)
            .Select(club => club.Id)
            .ToListAsync(cancellationToken);

        var playerIds = clubIds.Count == 0
            ? []
            : await dbContext.Players
                .Where(player => clubIds.Contains(player.ClubId))
                .Select(player => player.Id)
                .ToListAsync(cancellationToken);

        var lineups = await dbContext.Lineups
            .Where(lineup => lineup.GameSaveId == gameId)
            .ToListAsync(cancellationToken);

        var fixtures = await dbContext.Fixtures
            .Where(fixture => fixture.SeasonId == gameSave.SeasonId)
            .ToListAsync(cancellationToken);

        var financeEntries = clubIds.Count == 0
            ? []
            : await dbContext.FinanceEntries
                .Where(entry => clubIds.Contains(entry.ClubId))
                .ToListAsync(cancellationToken);

        var transfers = clubIds.Count == 0 && playerIds.Count == 0
            ? []
            : await dbContext.Transfers
                .Where(transfer => clubIds.Contains(transfer.FromClubId) ||
                                   clubIds.Contains(transfer.ToClubId) ||
                                   playerIds.Contains(transfer.PlayerId))
                .ToListAsync(cancellationToken);

        var players = playerIds.Count == 0
            ? []
            : await dbContext.Players
                .Where(player => playerIds.Contains(player.Id))
                .ToListAsync(cancellationToken);

        var clubs = clubIds.Count == 0
            ? []
            : await dbContext.Clubs
                .Where(club => clubIds.Contains(club.Id))
                .ToListAsync(cancellationToken);

        var league = await dbContext.Leagues
            .SingleOrDefaultAsync(candidate => candidate.Id == leagueId, cancellationToken);

        dbContext.Lineups.RemoveRange(lineups);
        dbContext.Fixtures.RemoveRange(fixtures);
        dbContext.FinanceEntries.RemoveRange(financeEntries);
        dbContext.Transfers.RemoveRange(transfers);
        dbContext.GameSaves.Remove(gameSave);
        dbContext.Players.RemoveRange(players);
        dbContext.Clubs.RemoveRange(clubs);
        dbContext.Seasons.Remove(gameSave.Season);

        if (league is not null)
        {
            dbContext.Leagues.Remove(league);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return deletedSave;
    }

    private static GameSaveSummaryDto MapSummary(
        Guid gameId,
        string saveName,
        string clubName,
        string seasonName,
        DateTime createdAt,
        DateTime lastSavedAt) =>
        new(
            gameId,
            saveName,
            clubName,
            seasonName,
            createdAt,
            lastSavedAt);
}
