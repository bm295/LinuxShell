namespace FootballManager.Application.Contracts;

public sealed record LineupDto(
    Guid FormationId,
    string FormationName,
    int StarterCount,
    int RequiredStarters,
    int AverageRating,
    int AverageFitness,
    int AverageMorale,
    string Readiness,
    IReadOnlyCollection<Guid> StarterPlayerIds);
