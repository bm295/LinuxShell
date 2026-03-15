using FootballManager.Application.Contracts;
using FootballManager.Domain.Entities;
using FootballManager.Domain.Enums;

namespace FootballManager.Infrastructure.Services.Game;

internal static class SquadViewFactory
{
    public static SquadSummaryDto BuildSquadSummary(IReadOnlyCollection<Player> players, IReadOnlySet<Guid> starterIds)
    {
        var squadPlayers = players
            .OrderBy(player => player.SquadNumber)
            .Select(player => MapPlayer(player, starterIds))
            .ToList();

        return new SquadSummaryDto(
            squadPlayers.Count,
            CalculateAverage(players.Select(player => player.GetOverallRating())),
            players.Count(player => player.Position == PlayerPosition.Goalkeeper),
            players.Count(player => player.Position == PlayerPosition.Defender),
            players.Count(player => player.Position == PlayerPosition.Midfielder),
            players.Count(player => player.Position == PlayerPosition.Forward),
            squadPlayers);
    }

    public static SquadPlayerDto MapPlayer(Player player, IReadOnlySet<Guid> starterIds) =>
        new(
            player.Id,
            player.FullName,
            player.Position.ToString(),
            player.Age,
            player.SquadNumber,
            player.Attack,
            player.Defense,
            player.Passing,
            player.Fitness,
            player.Morale,
            player.GetOverallRating(),
            player.IsCaptain,
            starterIds.Contains(player.Id),
            player.IsInjured,
            player.InjuryMatchesRemaining);

    public static LineupDto BuildLineup(Lineup lineup, IReadOnlyCollection<Player> squadPlayers)
    {
        var starterIds = lineup.GetStarterPlayerIds().ToList();
        var starterIdSet = starterIds.ToHashSet();
        var starters = squadPlayers
            .Where(player => starterIdSet.Contains(player.Id))
            .ToList();
        var requiredStarters = lineup.Formation?.RequiredStarters ?? 11;
        var readinessScore = CalculateAverage(starters.Select(player => player.GetReadinessScore()));

        return new LineupDto(
            lineup.FormationId,
            lineup.Formation?.Name ?? "Unassigned",
            starters.Count,
            requiredStarters,
            CalculateAverage(starters.Select(player => player.GetOverallRating())),
            CalculateAverage(starters.Select(player => player.Fitness)),
            CalculateAverage(starters.Select(player => player.Morale)),
            DescribeReadiness(starters.Count, requiredStarters, readinessScore),
            starterIds);
    }

    public static FeaturedPlayerDto BuildFeaturedPlayer(IReadOnlyCollection<Player> players, IReadOnlySet<Guid> starterIds)
    {
        var featuredPlayer = players
            .OrderByDescending(player => (starterIds.Contains(player.Id) ? 12 : 0) + player.GetOverallRating() + (player.GetReadinessScore() * 0.2))
            .ThenBy(player => player.SquadNumber)
            .First();

        var isStarter = starterIds.Contains(featuredPlayer.Id);
        var spotlight = featuredPlayer.IsInjured
            ? $"{featuredPlayer.FullName} is in recovery and the selection board has shifted around him."
            : isStarter
            ? $"{featuredPlayer.FullName} is carrying the tempo into the next matchday."
            : $"{featuredPlayer.FullName} is turning training into a selection headache.";

        return new FeaturedPlayerDto(
            featuredPlayer.Id,
            featuredPlayer.FullName,
            featuredPlayer.Position.ToString(),
            featuredPlayer.SquadNumber,
            featuredPlayer.GetOverallRating(),
            featuredPlayer.Fitness,
            featuredPlayer.Morale,
            isStarter,
            featuredPlayer.IsInjured,
            featuredPlayer.InjuryMatchesRemaining,
            spotlight);
    }

    public static PlayerDetailDto BuildPlayerDetail(Player player, IReadOnlySet<Guid> starterIds)
    {
        var isStarter = starterIds.Contains(player.Id);
        var roleStatus = player.IsInjured
            ? $"Unavailable for the next {player.InjuryMatchesRemaining} matchday(s)"
            : isStarter
            ? "Locked into the current XI"
            : player.GetReadinessScore() >= 80
                ? "Closing hard on the starting places"
                : "Rotation depth for the next selection call";

        return new PlayerDetailDto(
            player.Id,
            player.FullName,
            player.Position.ToString(),
            player.Age,
            player.SquadNumber,
            player.Attack,
            player.Defense,
            player.Passing,
            player.Fitness,
            player.Morale,
            player.GetOverallRating(),
            player.IsCaptain,
            isStarter,
            player.IsInjured,
            player.InjuryMatchesRemaining,
            roleStatus,
            BuildManagerNote(player));
    }

    private static string DescribeReadiness(int starterCount, int requiredStarters, int readinessScore)
    {
        if (starterCount < requiredStarters)
        {
            return "Selection incomplete";
        }

        return readinessScore switch
        {
            >= 86 => "Match sharp",
            >= 78 => "Ready for kickoff",
            >= 70 => "Still tuning the balance",
            _ => "Walking a fine line"
        };
    }

    private static string BuildManagerNote(Player player)
    {
        var strongestTrait = new[]
        {
            new KeyValuePair<string, int>("Attack", player.Attack),
            new KeyValuePair<string, int>("Defense", player.Defense),
            new KeyValuePair<string, int>("Passing", player.Passing)
        }
        .OrderByDescending(entry => entry.Value)
        .First()
        .Key;

        var traitNote = strongestTrait switch
        {
            "Attack" => "He changes the mood of a move the moment the ball breaks his way.",
            "Defense" => "He gives the back line its edge when the game turns rough.",
            _ => "He keeps the side stitched together when possession starts to wobble."
        };

        if (player.IsInjured)
        {
            return $"{traitNote} The medical room expects him back in {player.InjuryMatchesRemaining} matchday(s).";
        }

        if (player.Fitness < 70)
        {
            return $"{traitNote} The conditioning staff still want a little more in his legs.";
        }

        if (player.Morale >= 84)
        {
            return $"{traitNote} The training ground can feel his confidence right now.";
        }

        return traitNote;
    }

    private static int CalculateAverage(IEnumerable<int> values)
    {
        var materializedValues = values.ToList();
        return materializedValues.Count == 0
            ? 0
            : (int)Math.Round(materializedValues.Average(), MidpointRounding.AwayFromZero);
    }
}
