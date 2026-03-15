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
            .OrderBy(club => club.Name == "Arsenal" ? 0 : 1)
            .ThenBy(club => club.Name)
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
            .Include(league => league.Clubs)
                .ThenInclude(club => club.AcademyPlayers)
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
            Player? clonedCaptain = null;

            foreach (var templatePlayer in templateClub.Players.OrderBy(player => player.SquadNumber))
            {
                var clonedPlayer = clonedClub.AddPlayer(
                    templatePlayer.FirstName,
                    templatePlayer.LastName,
                    templatePlayer.Position,
                    templatePlayer.Age,
                    templatePlayer.SquadNumber,
                    templatePlayer.Attack,
                    templatePlayer.Defense,
                    templatePlayer.Passing,
                    templatePlayer.Fitness,
                    templatePlayer.Morale);

                if (templatePlayer.IsCaptain)
                {
                    clonedCaptain = clonedPlayer;
                }
            }

            if (clonedCaptain is not null)
            {
                clonedClub.SetCaptain(clonedCaptain);
            }
            else
            {
                clonedClub.EnsureCaptain();
            }

            foreach (var academyPlayer in templateClub.AcademyPlayers.OrderByDescending(player => player.Potential))
            {
                clonedClub.AddAcademyPlayer(
                    academyPlayer.FirstName,
                    academyPlayer.LastName,
                    academyPlayer.Position,
                    academyPlayer.Age,
                    academyPlayer.Attack,
                    academyPlayer.Defense,
                    academyPlayer.Passing,
                    academyPlayer.Fitness,
                    academyPlayer.Morale,
                    academyPlayer.Potential,
                    academyPlayer.DevelopmentProgress,
                    academyPlayer.TrainingFocus);
            }
        }

        var season = gameLeague.StartSeason("Season 1");
        RoundRobinFixtureGenerator.BuildSeasonFixtures(
            season,
            gameLeague.Clubs.OrderBy(club => club.Name).ToList());

        var selectedClub = clonedClubs[request.ClubId];
        var gameSave = new GameSave(selectedClub, season);
        var defaultFormation = await LineupPlanner.GetDefaultFormationAsync(dbContext, cancellationToken);
        var defaultStarters = LineupPlanner.SelectDefaultStarters(selectedClub, defaultFormation);
        gameSave.SetLineup(defaultFormation, defaultStarters.Select(player => player.Id));

        dbContext.Leagues.Add(gameLeague);
        dbContext.GameSaves.Add(gameSave);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateNewGameResponseDto(gameSave.Id, selectedClub.Name, season.Id, season.Name);
    }
}
