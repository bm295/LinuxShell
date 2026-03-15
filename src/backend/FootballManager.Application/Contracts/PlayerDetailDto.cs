namespace FootballManager.Application.Contracts;

public sealed record PlayerDetailDto(
    Guid Id,
    string Name,
    string Position,
    int Age,
    int SquadNumber,
    int Attack,
    int Defense,
    int Passing,
    int Fitness,
    int Morale,
    int OverallRating,
    bool IsCaptain,
    bool IsStarter,
    bool IsInjured,
    int InjuryMatchesRemaining,
    string RoleStatus,
    string ManagerNote);
