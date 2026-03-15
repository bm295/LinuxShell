namespace FootballManager.Application.Contracts;

public sealed record SimulatedMatchResultDto(
    string HomeTeam,
    string AwayTeam,
    MatchScoreDto Score,
    IReadOnlyCollection<MatchEventDto> MatchEvents,
    LeagueTableEntryDto ClubStanding,
    NextFixtureDto? NextFixture,
    string Summary);
