using FootballManager.Application.Contracts;

namespace FootballManager.Application.Services;

public interface IMatchSimulationService
{
    Task<SimulatedMatchResultDto?> SimulateNextMatchAsync(Guid gameId, CancellationToken cancellationToken = default);
}
