namespace FootballManager.Application.Contracts;

public sealed record SimulatedMatchResultDto(
    string HomeTeam,
    string AwayTeam,
    MatchScoreDto Score,
    IReadOnlyCollection<MatchEventDto> MatchEvents,
    MatchMvpDto MatchMvp,
    IReadOnlyCollection<PlayerDevelopmentChangeDto> SeniorPlayerDevelopment,
    IReadOnlyCollection<AcademyDevelopmentChangeDto> AcademyDevelopment,
    LeagueTableEntryDto ClubStanding,
    NextFixtureDto? NextFixture,
    string Summary);
