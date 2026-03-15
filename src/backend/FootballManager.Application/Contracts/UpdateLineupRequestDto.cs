namespace FootballManager.Application.Contracts;

public sealed record UpdateLineupRequestDto(Guid FormationId, IReadOnlyCollection<Guid> PlayerIds);
