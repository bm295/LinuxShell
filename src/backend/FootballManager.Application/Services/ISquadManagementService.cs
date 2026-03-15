using FootballManager.Application.Contracts;

namespace FootballManager.Application.Services;

public interface ISquadManagementService
{
    Task<IReadOnlyCollection<SquadPlayerDto>?> GetSquadAsync(Guid gameId, CancellationToken cancellationToken = default);

    Task<PlayerDetailDto?> GetPlayerAsync(Guid gameId, Guid playerId, CancellationToken cancellationToken = default);

    Task<LineupEditorDto?> GetLineupAsync(Guid gameId, CancellationToken cancellationToken = default);

    Task<LineupDto?> UpdateLineupAsync(Guid gameId, UpdateLineupRequestDto request, CancellationToken cancellationToken = default);
}
