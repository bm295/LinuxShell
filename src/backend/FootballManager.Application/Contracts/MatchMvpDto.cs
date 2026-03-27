namespace FootballManager.Application.Contracts;

public sealed record MatchMvpDto(
    Guid PlayerId,
    string PlayerName,
    string ClubName,
    string Position,
    int SquadNumber,
    int OverallRating,
    int MvpAwards);
