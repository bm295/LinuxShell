using FootballManager.Application.Contracts;

namespace FootballManager.Application.Services;

public interface IGameSetupService
{
    Task<IReadOnlyCollection<ClubOptionDto>> GetAvailableClubsAsync(CancellationToken cancellationToken = default);

    Task<CreateNewGameResponseDto> CreateNewGameAsync(CreateNewGameRequestDto request, CancellationToken cancellationToken = default);
}
