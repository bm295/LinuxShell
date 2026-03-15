namespace FootballManager.Application.Contracts;

public sealed record TransferMarketPlayerDto(
    Guid PlayerId,
    string Name,
    string Position,
    int SquadNumber,
    string ClubName,
    int OverallRating,
    int Fitness,
    int Morale,
    decimal Fee,
    bool IsAffordable);
