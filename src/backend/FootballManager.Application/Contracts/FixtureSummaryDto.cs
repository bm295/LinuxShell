namespace FootballManager.Application.Contracts;

public sealed record FixtureSummaryDto(
    Guid Id,
    string HomeClub,
    string AwayClub,
    int RoundNumber,
    DateTime ScheduledAt,
    bool IsPlayed,
    int? HomeGoals,
    int? AwayGoals,
    DateTime? PlayedAt,
    bool IsManagedClubFixture,
    bool IsCurrentRound);
