namespace FootballManager.Application.Contracts;

public sealed record AcademySummaryDto(
    string ClubName,
    int AcademyDepth,
    int PromotionReadyCount,
    int AveragePotential,
    int AverageReadiness,
    string SummaryNote,
    string PromotionPressure,
    AcademyPlayerDto? SpotlightPlayer,
    IReadOnlyCollection<AcademyPlayerDto> Players);
