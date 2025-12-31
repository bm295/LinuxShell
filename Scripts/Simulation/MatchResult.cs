using FootballManagerSim.Models;

namespace FootballManagerSim.Simulation;

public sealed record MatchResult(Club Home, Club Away, int HomeGoals, int AwayGoals);
