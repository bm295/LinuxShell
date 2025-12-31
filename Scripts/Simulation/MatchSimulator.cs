using System;
using FootballManagerSim.Models;

namespace FootballManagerSim.Simulation;

public sealed class MatchSimulator
{
    private readonly Random _random = new();

    public MatchResult PlayMatch(Club home, Club away)
    {
        var homeAdvantage = 0.15;
        var homeStrength = StrengthScore(home) + homeAdvantage;
        var awayStrength = StrengthScore(away);

        var homeGoals = SampleGoals(homeStrength);
        var awayGoals = SampleGoals(awayStrength);

        return new MatchResult(home, away, homeGoals, awayGoals);
    }

    private static double StrengthScore(Club club)
    {
        var reputationScore = club.Reputation / 100.0;
        var budgetScore = Math.Min(club.Budget / 10_000_000.0, 1.0);
        return (reputationScore * 0.7) + (budgetScore * 0.3);
    }

    private int SampleGoals(double strength)
    {
        var baseGoals = 1.2;
        var expectedGoals = baseGoals + (strength * 1.8);
        var variance = (_random.NextDouble() - 0.5) * 1.2;
        var goals = Math.Clamp(expectedGoals + variance, 0, 6);
        return (int)Math.Round(goals);
    }
}
