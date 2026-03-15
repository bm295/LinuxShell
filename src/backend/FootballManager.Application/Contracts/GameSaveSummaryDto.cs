namespace FootballManager.Application.Contracts;

public sealed record GameSaveSummaryDto(
    Guid GameId,
    string SaveName,
    string ClubName,
    string SeasonName,
    DateTime CreatedAt,
    DateTime LastSavedAt);
