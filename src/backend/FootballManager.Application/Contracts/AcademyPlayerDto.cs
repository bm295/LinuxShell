namespace FootballManager.Application.Contracts;

public sealed record AcademyPlayerDto(
    Guid PlayerId,
    string Name,
    string Position,
    int Age,
    int OverallRating,
    int Potential,
    int DevelopmentProgress,
    int PromotionReadiness,
    string TrainingFocus,
    string TrainingStatus,
    bool IsPromotionReady,
    string PromotionNote);
