namespace FootballManager.Application.Contracts;

public sealed record LineupEditorDto(
    string ClubName,
    LineupDto Lineup,
    IReadOnlyCollection<FormationDto> Formations,
    IReadOnlyCollection<SquadPlayerDto> Players);
