namespace FootballManager.Application.Contracts;

public sealed record ClubDashboardDto(
    string ClubName,
    string SeasonName,
    decimal Budget,
    int LeaguePosition,
    int Points,
    NextFixtureDto? NextFixture,
    SquadSummaryDto SquadSummary);
