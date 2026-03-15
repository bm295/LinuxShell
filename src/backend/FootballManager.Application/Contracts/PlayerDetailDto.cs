namespace FootballManager.Application.Contracts;

public sealed record PlayerDetailDto(
    Guid Id,
    string Name,
    string Position,
    int SquadNumber,
    int Attack,
    int Defense,
    int Passing,
    int Fitness,
    int Morale,
    int OverallRating,
    bool IsStarter,
    string RoleStatus,
    string ManagerNote);
