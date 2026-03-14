using FootballManager.Application.Contracts;

namespace FootballManager.Application.Services;

public interface IBootstrapSummaryService
{
    Task<BootstrapSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
