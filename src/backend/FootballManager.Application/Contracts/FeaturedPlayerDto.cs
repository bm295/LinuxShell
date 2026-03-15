namespace FootballManager.Application.Contracts;

public sealed record FeaturedPlayerDto(
    Guid Id,
    string Name,
    string Position,
    int SquadNumber,
    int OverallRating,
    int Fitness,
    int Morale,
    bool IsStarter,
    string Spotlight);
