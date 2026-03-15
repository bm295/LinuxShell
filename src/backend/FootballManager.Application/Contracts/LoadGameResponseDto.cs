namespace FootballManager.Application.Contracts;

public sealed record LoadGameResponseDto(
    GameSaveSummaryDto? SelectedSave,
    IReadOnlyCollection<GameSaveSummaryDto> Saves);
