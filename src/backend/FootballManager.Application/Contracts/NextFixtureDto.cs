namespace FootballManager.Application.Contracts;

public sealed record NextFixtureDto(string HomeClub, string AwayClub, DateTime ScheduledAt, int RoundNumber);
