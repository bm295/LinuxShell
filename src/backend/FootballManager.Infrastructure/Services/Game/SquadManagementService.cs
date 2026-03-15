using FootballManager.Application.Contracts;
using FootballManager.Application.Services;
using FootballManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FootballManager.Infrastructure.Services.Game;

public sealed class SquadManagementService(FootballManagerDbContext dbContext) : ISquadManagementService
{
    public async Task<IReadOnlyCollection<SquadPlayerDto>?> GetSquadAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        var gameSave = await LoadGameSaveAsync(gameId, cancellationToken);
        if (gameSave?.SelectedClub is null)
        {
            return null;
        }

        var lineup = await LineupPlanner.EnsureLineupAsync(dbContext, gameSave, cancellationToken);
        var starterIds = lineup.GetStarterPlayerIds().ToHashSet();

        return gameSave.SelectedClub.Players
            .OrderBy(player => player.SquadNumber)
            .Select(player => SquadViewFactory.MapPlayer(player, starterIds))
            .ToList();
    }

    public async Task<PlayerDetailDto?> GetPlayerAsync(Guid gameId, Guid playerId, CancellationToken cancellationToken = default)
    {
        var gameSave = await LoadGameSaveAsync(gameId, cancellationToken);
        if (gameSave?.SelectedClub is null)
        {
            return null;
        }

        var lineup = await LineupPlanner.EnsureLineupAsync(dbContext, gameSave, cancellationToken);
        var player = gameSave.SelectedClub.Players.SingleOrDefault(candidate => candidate.Id == playerId);
        return player is null
            ? null
            : SquadViewFactory.BuildPlayerDetail(player, lineup.GetStarterPlayerIds().ToHashSet());
    }

    public async Task<LineupEditorDto?> GetLineupAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        var gameSave = await LoadGameSaveAsync(gameId, cancellationToken);
        if (gameSave?.SelectedClub is null)
        {
            return null;
        }

        var lineup = await LineupPlanner.EnsureLineupAsync(dbContext, gameSave, cancellationToken);
        var starterIds = lineup.GetStarterPlayerIds().ToHashSet();
        var formations = await dbContext.Formations
            .AsNoTracking()
            .OrderBy(formation => formation.Name)
            .Select(formation => new FormationDto(
                formation.Id,
                formation.Name,
                formation.Defenders,
                formation.Midfielders,
                formation.Forwards))
            .ToListAsync(cancellationToken);

        return new LineupEditorDto(
            gameSave.SelectedClub.Name,
            SquadViewFactory.BuildLineup(lineup, gameSave.SelectedClub.Players.ToList()),
            formations,
            gameSave.SelectedClub.Players
                .OrderBy(player => player.SquadNumber)
                .Select(player => SquadViewFactory.MapPlayer(player, starterIds))
                .ToList());
    }

    public async Task<LineupDto?> UpdateLineupAsync(
        Guid gameId,
        UpdateLineupRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var gameSave = await LoadGameSaveAsync(gameId, cancellationToken);
        if (gameSave?.SelectedClub is null)
        {
            return null;
        }

        var formation = await dbContext.Formations.SingleOrDefaultAsync(
            candidate => candidate.Id == request.FormationId,
            cancellationToken);

        if (formation is null)
        {
            throw new InvalidOperationException("Selected formation was not found.");
        }

        var starters = LineupPlanner.ResolveAndValidateStarters(gameSave.SelectedClub, formation, request.PlayerIds);
        var lineup = gameSave.SetLineup(formation, starters.Select(player => player.Id));
        await dbContext.SaveChangesAsync(cancellationToken);

        return SquadViewFactory.BuildLineup(lineup, gameSave.SelectedClub.Players.ToList());
    }

    private async Task<FootballManager.Domain.Entities.GameSave?> LoadGameSaveAsync(Guid gameId, CancellationToken cancellationToken)
    {
        return await dbContext.GameSaves
            .Include(save => save.SelectedClub)
                .ThenInclude(club => club!.Players)
            .Include(save => save.Lineup)
                .ThenInclude(lineup => lineup!.Formation)
            .SingleOrDefaultAsync(save => save.Id == gameId, cancellationToken);
    }
}
