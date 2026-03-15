namespace FootballManager.Application.Contracts;

public sealed record ClubDashboardDto(
    string ClubName,
    string SeasonName,
    string CompetitionName,
    decimal Budget,
    int LeaguePosition,
    int Points,
    NextFixtureDto? NextFixture,
    RecentResultDto? RecentResult,
    string MomentumNote,
    SquadSummaryDto SquadSummary,
    LineupDto Lineup,
    FeaturedPlayerDto FeaturedPlayer);
