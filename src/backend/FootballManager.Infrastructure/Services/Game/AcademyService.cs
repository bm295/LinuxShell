using FootballManager.Application.Contracts;
using FootballManager.Application.Services;
using FootballManager.Domain.Entities;
using FootballManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FootballManager.Infrastructure.Services.Game;

public sealed class AcademyService(FootballManagerDbContext dbContext) : IAcademyService
{
    public async Task<AcademySummaryDto?> GetAcademyAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        var gameSave = await LoadGameSaveAsync(gameId, cancellationToken);
        if (gameSave?.SelectedClub is null)
        {
            return null;
        }

        return BuildSummary(gameSave.SelectedClub);
    }

    public async Task<AcademyPromotionResultDto?> PromotePlayerAsync(
        Guid gameId,
        PromoteAcademyPlayerRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var gameSave = await LoadGameSaveAsync(gameId, cancellationToken);
        if (gameSave?.SelectedClub is null)
        {
            return null;
        }

        var academyPlayer = gameSave.SelectedClub.AcademyPlayers
            .SingleOrDefault(player => player.Id == request.AcademyPlayerId);

        if (academyPlayer is null)
        {
            return null;
        }

        var selectedClub = gameSave.SelectedClub;
        var squadNumber = selectedClub.GetNextAvailableSquadNumber();
        var seniorPlayer = selectedClub.AddPlayer(
            academyPlayer.FirstName,
            academyPlayer.LastName,
            academyPlayer.Position,
            academyPlayer.Age,
            squadNumber,
            academyPlayer.Attack,
            academyPlayer.Defense,
            academyPlayer.Passing,
            academyPlayer.Fitness,
            academyPlayer.Morale);

        selectedClub.AcademyPlayers.Remove(academyPlayer);
        dbContext.AcademyPlayers.Remove(academyPlayer);
        selectedClub.EnsureCaptain();
        await dbContext.SaveChangesAsync(cancellationToken);

        return new AcademyPromotionResultDto(
            academyPlayer.Id,
            seniorPlayer.Id,
            seniorPlayer.FullName,
            seniorPlayer.SquadNumber,
            selectedClub.AcademyPlayers.Count,
            selectedClub.Players.Count,
            $"{seniorPlayer.FullName} steps out of the academy and into the senior squad wearing number {seniorPlayer.SquadNumber}.");
    }

    private async Task<GameSave?> LoadGameSaveAsync(Guid gameId, CancellationToken cancellationToken)
    {
        return await dbContext.GameSaves
            .Include(save => save.SelectedClub)
                .ThenInclude(club => club!.Players)
            .Include(save => save.SelectedClub)
                .ThenInclude(club => club!.AcademyPlayers)
            .SingleOrDefaultAsync(save => save.Id == gameId, cancellationToken);
    }

    private static AcademySummaryDto BuildSummary(Club club)
    {
        var academyPlayers = club.AcademyPlayers
            .OrderByDescending(player => player.GetPromotionReadiness())
            .ThenByDescending(player => player.Potential)
            .ThenBy(player => player.LastName)
            .ThenBy(player => player.FirstName)
            .ToList();
        var promotionReadyCount = academyPlayers.Count(player => player.IsPromotionReady());
        var averagePotential = academyPlayers.Count == 0
            ? 0
            : (int)Math.Round(academyPlayers.Average(player => player.Potential), MidpointRounding.AwayFromZero);
        var averageReadiness = academyPlayers.Count == 0
            ? 0
            : (int)Math.Round(academyPlayers.Average(player => player.GetPromotionReadiness()), MidpointRounding.AwayFromZero);
        var spotlightPlayer = academyPlayers.FirstOrDefault();

        return new AcademySummaryDto(
            club.Name,
            academyPlayers.Count,
            promotionReadyCount,
            averagePotential,
            averageReadiness,
            BuildSummaryNote(club, academyPlayers, promotionReadyCount),
            BuildPromotionPressure(club, spotlightPlayer),
            spotlightPlayer is null ? null : MapPlayer(spotlightPlayer),
            academyPlayers.Select(MapPlayer).ToList());
    }

    private static AcademyPlayerDto MapPlayer(AcademyPlayer player) =>
        new(
            player.Id,
            player.FullName,
            player.Position.ToString(),
            player.Age,
            player.GetOverallRating(),
            player.Potential,
            player.DevelopmentProgress,
            player.GetPromotionReadiness(),
            player.TrainingFocus,
            player.GetTrainingStatus(),
            player.IsPromotionReady(),
            BuildPromotionNote(player));

    private static string BuildSummaryNote(Club club, IReadOnlyCollection<AcademyPlayer> academyPlayers, int promotionReadyCount)
    {
        if (academyPlayers.Count == 0)
        {
            return $"{club.Name} are waiting on the next academy intake before the long-term picture changes.";
        }

        if (promotionReadyCount >= 2)
        {
            return $"{promotionReadyCount} young players are forcing the question. The academy can start shaping first-team decisions right now.";
        }

        if (academyPlayers.Any(player => player.Potential >= 84))
        {
            return "There is real upside in the academy. The next breakthrough feels more like timing than hope.";
        }

        return "The academy is quietly building future options behind the current XI. Patience can still turn into value here.";
    }

    private static string BuildPromotionPressure(Club club, AcademyPlayer? spotlightPlayer)
    {
        if (spotlightPlayer is null)
        {
            return "No one is close enough to disrupt the first-team depth chart yet.";
        }

        var seniorDepthAtPosition = club.Players.Count(player => player.Position == spotlightPlayer.Position);
        if (spotlightPlayer.IsPromotionReady() && seniorDepthAtPosition <= 4)
        {
            return $"{spotlightPlayer.FullName} is close enough to solve a squad need internally at {spotlightPlayer.Position}.";
        }

        if (spotlightPlayer.IsPromotionReady())
        {
            return $"{spotlightPlayer.FullName} is pressing hard enough to challenge the transfer shortlist and demand a senior look.";
        }

        return $"{spotlightPlayer.FullName} is the leading prospect, but one more stretch of development would sharpen the promotion case.";
    }

    private static string BuildPromotionNote(AcademyPlayer player)
    {
        if (player.IsPromotionReady())
        {
            return "Ready to test senior minutes.";
        }

        if (player.Potential >= 84)
        {
            return "High-upside talent who still needs time on the training pitch.";
        }

        return "Developing steadily in the academy setup.";
    }
}
