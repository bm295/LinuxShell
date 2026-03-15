namespace FootballManager.Application.Contracts;

public sealed record LeagueTableEntryDto(
    Guid ClubId,
    string ClubName,
    int Position,
    int Played,
    int Wins,
    int Draws,
    int Losses,
    int GoalsFor,
    int GoalsAgainst,
    int GoalDifference,
    int Points);
