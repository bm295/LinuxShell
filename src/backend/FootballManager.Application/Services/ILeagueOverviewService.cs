using FootballManager.Application.Contracts;

namespace FootballManager.Application.Services;

public interface ILeagueOverviewService
{
    Task<IReadOnlyCollection<LeagueTableEntryDto>?> GetLeagueTableAsync(Guid gameId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<FixtureSummaryDto>?> GetFixturesAsync(Guid gameId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TopPlayerDto>?> GetTopPlayersAsync(Guid gameId, CancellationToken cancellationToken = default);
}
