using FootballManager.Application.Contracts;

namespace FootballManager.Application.Services;

public interface IGameSaveService
{
    Task<GameSaveSummaryDto?> SaveAsync(Guid gameId, SaveGameRequestDto request, CancellationToken cancellationToken = default);

    Task<LoadGameResponseDto> LoadAsync(Guid? gameId, CancellationToken cancellationToken = default);

    Task<GameSaveSummaryDto?> DeleteAsync(Guid gameId, CancellationToken cancellationToken = default);
}
