using FootballManager.Domain.Entities;
using FootballManager.Domain.Enums;
using FootballManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FootballManager.Infrastructure.Services.Game;

internal static class LineupPlanner
{
    private const string DefaultFormationName = "4-3-3";

    public static async Task<Lineup> EnsureLineupAsync(
        FootballManagerDbContext dbContext,
        GameSave gameSave,
        CancellationToken cancellationToken = default)
    {
        if (gameSave.SelectedClub is null)
        {
            throw new InvalidOperationException("The selected club is required before a lineup can be created.");
        }

        if (gameSave.Lineup?.Formation is not null)
        {
            return gameSave.Lineup;
        }

        var defaultFormation = await GetDefaultFormationAsync(dbContext, cancellationToken);
        var starters = SelectDefaultStarters(gameSave.SelectedClub, defaultFormation);
        var lineup = gameSave.SetLineup(defaultFormation, starters.Select(player => player.Id));
        await dbContext.SaveChangesAsync(cancellationToken);
        return lineup;
    }

    public static async Task<Formation> GetDefaultFormationAsync(
        FootballManagerDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Formations.SingleOrDefaultAsync(
                   formation => formation.Name == DefaultFormationName,
                   cancellationToken)
               ?? throw new InvalidOperationException("Default formation data is unavailable.");
    }

    public static IReadOnlyCollection<Player> SelectDefaultStarters(Club club, Formation formation)
    {
        ArgumentNullException.ThrowIfNull(club);
        ArgumentNullException.ThrowIfNull(formation);

        var starters = new List<Player>();
        starters.AddRange(PickPlayers(club, PlayerPosition.Goalkeeper, formation.Goalkeepers));
        starters.AddRange(PickPlayers(club, PlayerPosition.Defender, formation.Defenders));
        starters.AddRange(PickPlayers(club, PlayerPosition.Midfielder, formation.Midfielders));
        starters.AddRange(PickPlayers(club, PlayerPosition.Forward, formation.Forwards));
        return starters;
    }

    public static IReadOnlyCollection<Player> ResolveAndValidateStarters(
        Club club,
        Formation formation,
        IEnumerable<Guid> starterPlayerIds)
    {
        ArgumentNullException.ThrowIfNull(club);
        ArgumentNullException.ThrowIfNull(formation);

        if (starterPlayerIds is null)
        {
            throw new ArgumentNullException(nameof(starterPlayerIds));
        }

        var starterIds = starterPlayerIds
            .Distinct()
            .ToList();

        if (starterIds.Count != formation.RequiredStarters)
        {
            throw new InvalidOperationException($"Choose exactly {formation.RequiredStarters} starters for {formation.Name}.");
        }

        var playersById = club.Players.ToDictionary(player => player.Id);
        if (starterIds.Any(starterId => !playersById.ContainsKey(starterId)))
        {
            throw new InvalidOperationException("Lineup can only include players from the active club squad.");
        }

        var starters = starterIds
            .Select(starterId => playersById[starterId])
            .ToList();

        ValidatePositionCount(starters, PlayerPosition.Goalkeeper, formation.Goalkeepers, formation.Name);
        ValidatePositionCount(starters, PlayerPosition.Defender, formation.Defenders, formation.Name);
        ValidatePositionCount(starters, PlayerPosition.Midfielder, formation.Midfielders, formation.Name);
        ValidatePositionCount(starters, PlayerPosition.Forward, formation.Forwards, formation.Name);

        return starters;
    }

    private static IReadOnlyCollection<Player> PickPlayers(Club club, PlayerPosition position, int requiredCount)
    {
        var players = club.Players
            .Where(player => player.Position == position)
            .OrderByDescending(player => GetStarterScore(player))
            .ThenBy(player => player.SquadNumber)
            .Take(requiredCount)
            .ToList();

        if (players.Count != requiredCount)
        {
            throw new InvalidOperationException($"The squad does not have enough {position.ToString().ToLowerInvariant()}s to fill the lineup.");
        }

        return players;
    }

    private static double GetStarterScore(Player player) =>
        player.GetOverallRating() + (player.GetReadinessScore() * 0.35);

    private static void ValidatePositionCount(
        IReadOnlyCollection<Player> starters,
        PlayerPosition position,
        int requiredCount,
        string formationName)
    {
        var actualCount = starters.Count(player => player.Position == position);

        if (actualCount != requiredCount)
        {
            throw new InvalidOperationException(
                $"{formationName} requires {requiredCount} {position.ToString().ToLowerInvariant()}(s).");
        }
    }
}
