namespace FootballManager.Application.Contracts;

public sealed record RecentResultDto(
    string HomeClub,
    string AwayClub,
    int HomeGoals,
    int AwayGoals,
    DateTime PlayedAt,
    int RoundNumber);
