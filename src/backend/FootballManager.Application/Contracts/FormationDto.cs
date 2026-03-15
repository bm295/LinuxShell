namespace FootballManager.Application.Contracts;

public sealed record FormationDto(Guid Id, string Name, int Defenders, int Midfielders, int Forwards);
