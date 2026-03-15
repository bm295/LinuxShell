using FootballManager.Application.Contracts;

namespace FootballManager.Application.Services;

public interface ITransferMarketService
{
    Task<TransferMarketDto?> GetMarketAsync(Guid gameId, CancellationToken cancellationToken = default);

    Task<TransferActionResultDto?> BuyPlayerAsync(Guid gameId, BuyTransferRequestDto request, CancellationToken cancellationToken = default);

    Task<TransferActionResultDto?> SellPlayerAsync(Guid gameId, SellTransferRequestDto request, CancellationToken cancellationToken = default);
}
