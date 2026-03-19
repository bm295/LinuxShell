namespace FootballManager.Application.Contracts;

public sealed record PlayerDevelopmentChangeDto(
    Guid PlayerId,
    string Name,
    string Position,
    int Age,
    int SquadNumber,
    bool IsCaptain,
    bool PlayedMatch,
    int OverallRating,
    int OverallDelta,
    int Attack,
    int AttackDelta,
    int Defense,
    int DefenseDelta,
    int Passing,
    int PassingDelta,
    int Fitness,
    int FitnessDelta,
    int Morale,
    int MoraleDelta);
