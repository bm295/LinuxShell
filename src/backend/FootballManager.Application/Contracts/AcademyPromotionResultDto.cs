namespace FootballManager.Application.Contracts;

public sealed record AcademyPromotionResultDto(
    Guid AcademyPlayerId,
    Guid SeniorPlayerId,
    string PlayerName,
    int SquadNumber,
    int AcademyDepth,
    int SeniorSquadCount,
    string Summary);
