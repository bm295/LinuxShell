using FootballManager.Application.Contracts;

namespace FootballManager.Application.Services;

public interface IAcademyService
{
    Task<AcademySummaryDto?> GetAcademyAsync(Guid gameId, CancellationToken cancellationToken = default);

    Task<AcademyPromotionResultDto?> PromotePlayerAsync(
        Guid gameId,
        PromoteAcademyPlayerRequestDto request,
        CancellationToken cancellationToken = default);
}
