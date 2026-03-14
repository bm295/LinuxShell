namespace FootballManager.Application.Contracts;

public sealed record ClubDashboardDto(
    string ClubName,
    decimal Budget,
    int LeaguePosition,
    int Points,
    NextFixtureDto? NextFixture,
    SquadSummaryDto SquadSummary);
