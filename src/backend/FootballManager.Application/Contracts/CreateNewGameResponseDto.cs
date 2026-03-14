namespace FootballManager.Application.Contracts;

public sealed record CreateNewGameResponseDto(Guid GameId, string SelectedClub, Guid SeasonId, string SeasonName);
