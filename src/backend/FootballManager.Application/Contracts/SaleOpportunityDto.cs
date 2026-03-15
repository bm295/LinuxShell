namespace FootballManager.Application.Contracts;

public sealed record SaleOpportunityDto(
    Guid PlayerId,
    string Name,
    string Position,
    int SquadNumber,
    int OverallRating,
    int Fitness,
    int Morale,
    decimal Fee,
    string SuggestedBuyer);
