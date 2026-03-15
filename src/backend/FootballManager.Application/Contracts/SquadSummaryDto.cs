namespace FootballManager.Application.Contracts;

public sealed record SquadSummaryDto(
    int TotalPlayers,
    int AverageRating,
    int Goalkeepers,
    int Defenders,
    int Midfielders,
    int Forwards,
    IReadOnlyCollection<SquadPlayerDto> Players);
