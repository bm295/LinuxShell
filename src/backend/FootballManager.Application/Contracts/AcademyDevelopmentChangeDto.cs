namespace FootballManager.Application.Contracts;

public sealed record AcademyDevelopmentChangeDto(
    Guid PlayerId,
    string Name,
    string Position,
    int Age,
    string TrainingFocus,
    int OverallRating,
    int OverallDelta,
    int Attack,
    int AttackDelta,
    int Defense,
    int DefenseDelta,
    int Passing,
    int PassingDelta,
    int Fitness,
    int FitnessDelta,
    int Morale,
    int MoraleDelta,
    int DevelopmentProgress,
    int DevelopmentProgressDelta);
