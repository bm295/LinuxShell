using FootballManager.Application.Contracts;

namespace FootballManager.Application.Services;

public interface IFinanceService
{
    Task<FinanceSummaryDto?> GetSummaryAsync(Guid gameId, CancellationToken cancellationToken = default);
}
