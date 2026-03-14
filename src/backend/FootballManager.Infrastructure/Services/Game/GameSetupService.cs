using FootballManager.Application.Contracts;
using FootballManager.Application.Services;
using FootballManager.Domain.Entities;
using FootballManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FootballManager.Infrastructure.Services.Game;

public sealed class GameSetupService(FootballManagerDbContext dbContext) : IGameSetupService
{
    public async Task<IReadOnlyCollection<ClubOptionDto>> GetAvailableClubsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Clubs
            .AsNoTracking()
            .Where(club => club.League != null && club.League.IsTemplate)
            .OrderBy(club => club.Name)
            .Select(club => new ClubOptionDto(club.Id, club.Name, club.TransferBudget))
            .ToListAsync(cancellationToken);
    }

    public async Task<CreateNewGameResponseDto> CreateNewGameAsync(
        CreateNewGameRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var templateLeague = await dbContext.Leagues
            .Include(league => league.Clubs)
                .ThenInclude(club => club.Players)
            .AsSplitQuery()
            .SingleOrDefaultAsync(league => league.IsTemplate, cancellationToken);

        if (templateLeague is null)
        {
            throw new InvalidOperationException("Template league data is not available.");
        }

        if (!templateLeague.Clubs.Any(club => club.Id == request.ClubId))
        {
            throw new KeyNotFoundException("Selected club was not found in the template league.");
        }

        var gameLeague = new League(templateLeague.Name);
        var clonedClubs = new Dictionary<Guid, Club>();

        foreach (var templateClub in templateLeague.Clubs.OrderBy(club => club.Name))
        {
            var clonedClub = gameLeague.AddClub(templateClub.Name, templateClub.TransferBudget);
            clonedClubs[templateClub.Id] = clonedClub;

            foreach (var templatePlayer in templateClub.Players.OrderBy(player => player.SquadNumber))
            {
                clonedClub.AddPlayer(
                    templatePlayer.FirstName,
                    templatePlayer.LastName,
                    templatePlayer.Position,
                    templatePlayer.SquadNumber);
            }
        }

        var gameNumber = await dbContext.GameSaves.CountAsync(cancellationToken) + 1;
        var season = gameLeague.StartSeason($"Season {gameNumber}");
        RoundRobinFixtureGenerator.BuildSeasonFixtures(
            season,
            gameLeague.Clubs.OrderBy(club => club.Name).ToList());

        var selectedClub = clonedClubs[request.ClubId];
        var gameSave = new GameSave(selectedClub, season);

        dbContext.Leagues.Add(gameLeague);
        dbContext.GameSaves.Add(gameSave);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateNewGameResponseDto(gameSave.Id, selectedClub.Name, season.Id, season.Name);
    }
}
