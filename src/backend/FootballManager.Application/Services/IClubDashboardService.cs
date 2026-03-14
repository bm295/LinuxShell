using FootballManager.Application.Contracts;

namespace FootballManager.Application.Services;

public interface IClubDashboardService
{
    Task<ClubDashboardDto?> GetDashboardAsync(Guid gameId, CancellationToken cancellationToken = default);
}
