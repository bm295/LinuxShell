namespace FootballManager.Application.Contracts;

public sealed record StartNextSeasonResultDto(
    Guid SeasonId,
    string SeasonName,
    NextFixtureDto? NextFixture,
    string Summary);
