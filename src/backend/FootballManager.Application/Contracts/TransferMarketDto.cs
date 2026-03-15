namespace FootballManager.Application.Contracts;

public sealed record TransferMarketDto(
    decimal AvailableBudget,
    IReadOnlyCollection<TransferMarketPlayerDto> Targets,
    IReadOnlyCollection<SaleOpportunityDto> SaleOpportunities,
    IReadOnlyCollection<TransferActivityDto> RecentActivity);
